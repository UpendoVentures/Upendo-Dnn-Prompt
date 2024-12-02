#region License

// Distributed under the MIT License
// ============================================================
// Copyright (c) Upendo Ventures, LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, 
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or 
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.

#endregion

using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using DotNetNuke.Instrumentation;
using Upendo.Modules.UpendoPrompt.Components;
using Constants = Upendo.Modules.UpendoPrompt.Components.Constants;

// required for DNN Prompt
//using DotNetNuke.Prompt; // deprecated Prompt is replaced with this
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data; // added for the commented-out clearing of cache below
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Log.EventLog;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("password-reset", Constants.PromptCategory, "ResetPassword")]
    public class ResetPassword : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ResetPassword));

        #region Constants

        private const string ParamCurrent = "current";
        private const string ParamAll = "all";
        private const string ParamMessage = "Message";

        // UpdateAllUsers()
        private const string QueryAllUsers = @"UPDATE {databaseOwner}[{objectQualifier}Users] SET [UpdatePassword] = 1 WHERE [IsSuperUser] = 0;";

        // UpdateAllUsersByPortal()
        private const string QueryUsersByPortal = @"
                UPDATE {databaseOwner}[{objectQualifier}Users] 
                SET [UpdatePassword] = 1 
                WHERE [IsSuperUser] = 0 AND [UserID] IN 
                (SELECT up.[UserID] FROM {databaseOwner}[{objectQualifier}UserPortals] up WHERE up.[PortalID] = @PortalID);";

        // UpdateSingleUser()
        private const string QuerySingleUser = @"
                UPDATE {databaseOwner}[{objectQualifier}Users] 
                SET [UpdatePassword] = 1 
                WHERE [IsSuperUser] = 0 AND [UserID] = @UserID;";

        // UpdateUsersForRole()
        private const string QueryUsersByRole = @"
                UPDATE [dbo].[Users] 
                SET [UpdatePassword] = 1 
                WHERE [IsSuperUser] = 0 AND [UserID] IN 
                (SELECT ur.[UserID] FROM {databaseOwner}[{objectQualifier}UserRoles] ur WHERE ur.[RoleID] = @RoleID);";

        private const string QueryGetRolePortalID = @"SELECT [PortalID] FROM {databaseOwner}[{objectQualifier}Roles] WHERE [RoleId] = @RoleId;";

        #endregion

        #region Properties

        [FlagParameter("scope", Constants.LocalizationKeys.FlagScope, "String", "", false)]
        private const string FlagScope = "scope";
        private string ParamScope { get; set; }

        [FlagParameter("userid", Constants.LocalizationKeys.FlagUserId, "Integer", "-1", false)]
        private const string FlagUserId = "userid";
        private int ParamUserID { get; set; }

        [FlagParameter("roleid", Constants.LocalizationKeys.FlagUserId, "Integer", "-1", false)]
        private const string FlagRoleId = "roleid";
        private int ParamRoleID { get; set; }

        private int RoleID
        {
            get {
                return this.ParamRoleID;
            }
        }
        
        [FlagParameter("portalid", Constants.LocalizationKeys.FlagPortalId, "Integer", "-1", false)]
        private const string FlagPortalId = "portalid";
        private int ParamPortalID { get; set; }

        private UserInfo currentUser { get; set; }

        #endregion

        #region Implementation

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            // get the command flags, if present
            this.ParamScope = this.GetFlagValue(FlagScope, "scope", string.Empty, false); // current portal (default) | all portals
            this.ParamUserID = this.GetFlagValue(FlagUserId, "userid", Null.NullInteger, false);
            this.ParamRoleID = this.GetFlagValue(FlagRoleId, "roleid", Null.NullInteger, false);
            this.ParamPortalID = this.GetFlagValue(FlagPortalId, "portalid", Null.NullInteger, false);

            // perform validation
            if (!ValidateFlags())
            {
                this.AddMessage(this.LocalizeString(Constants.LocalizationKeys.ResetPasswordFlagsInvalid));
            }
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var output = string.Empty;

                //-------------------------------------------------------------//
                // Verify the given flag values before we run any code
                //-------------------------------------------------------------//
                if (string.IsNullOrEmpty(this.ParamScope))
                {
                    // FLAG CHECK:  --userid
                    if (this.ParamUserID > Null.NullInteger)
                    {
                        UserInfo user = null;

                        user = DotNetNuke.Entities.Users.UserController.Instance.GetUserById(this.PortalId,
                            this.ParamUserID);
                        
                        if (user == null || user.UserID == Null.NullInteger)
                        {
                            // no user was found 
                            output = this.LocalizeString(Constants.LocalizationKeys.PasswordResetNoUserFound);

                            return new ConsoleResultModel
                            {
                                Output = output,
                                IsError = true
                            };
                        }
                    }

                    // FLAG CHECK:  --roleid
                    if (this.ParamRoleID > Null.NullInteger)
                    {
                        int rolePortalID = GetPortalIdForRole();

                        RoleInfo role = null;

                        role = RoleController.Instance.GetRoleById(rolePortalID, this.ParamRoleID); 

                        if (role == null || role.RoleID == Null.NullInteger)
                        {
                            // no role was found 
                            output = this.LocalizeString(Constants.LocalizationKeys.PasswordResetNoRoleFound);

                            return new ConsoleResultModel
                            {
                                Output = output,
                                IsError = true
                            };
                        }
                    }

                    // FLAG CHECK:  --portalid
                    if (this.ParamPortalID > Null.NullInteger)
                    {
                        PortalInfo portal = null;

                        portal = PortalController.Instance.GetPortal(this.ParamPortalID);

                        if (portal == null || portal.PortalID == Null.NullInteger)
                        {
                            // no portal was found 
                            output = this.LocalizeString(Constants.LocalizationKeys.PasswordResetNoPortalFound);

                            return new ConsoleResultModel
                            {
                                Output = output,
                                IsError = true
                            };
                        }
                    }
                }

                //---------------------------------------------------------------//
                // PERMISSION (RE)VALIDATION (redundant, because, Prompt, but...)
                //---------------------------------------------------------------//

                // verify the current user is indeed still authorized to do this  
                this.currentUser = UserController.Instance.GetCurrentUserInfo();
                if (currentUser == null || currentUser.UserID == -1 || !currentUser.IsSuperUser)
                {
                    output = this.LocalizeString(Constants.LocalizationKeys.ResetPasswordNotAuthorized);

                    return new ConsoleResultModel
                    {
                        Output = output,
                        IsError = true
                    };
                }

                //-------------------------------------------------------------//
                // Reset the password(s) for the specified user account(s)
                //-------------------------------------------------------------//

                RunPasswordReset();

                //-------------------------------------------------------------//
                // Let the end-user know the result
                //-------------------------------------------------------------//

                output = this.LocalizeString(Constants.LocalizationKeys.ResetPasswordSuccess);

                return new ConsoleResultModel
                {
                    Output = output,
                    IsError = false
                };
            }
            catch (Exception e)
            {
                LogError(e);
                return new ConsoleErrorResultModel(string.Concat(Constants.OutputPrefix, this.LocalizeString(Constants.LocalizationKeys.ErrorOccurred)));
            }
        }

        #endregion

        #region Helpers

        private void RunPasswordReset()
        {
            try
            {
                //Log event
                var log = new LogInfo
                {
                    BypassBuffering = true,
                    LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString(),
                };
                log.AddProperty(ParamMessage,
                    string.Format(this.LocalizeString(Constants.LocalizationKeys.PasswordsResetted),
                        this.currentUser.Username, this.ParamScope, this.ParamUserID, this.ParamRoleID,
                        this.ParamPortalID));
                LogController.Instance.AddLog(log);

                // update the passwords, as specified in the flags 
                UpdateUsersForPasswordReset();

                // clear cache 
                DataCache.ClearCache();
                DotNetNuke.Web.Client.ClientResourceManagement.ClientResourceManager.ClearCache();
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
        }

        private void UpdateUsersForPasswordReset()
        {
            if (!string.IsNullOrEmpty(this.ParamScope))
            {
                if (this.ParamScope == ParamCurrent)
                {
                    this.ParamPortalID = this.ParamPortalID;
                    UpdateAllUsersByPortal();
                    return;
                }
                else if (this.ParamScope == ParamAll)
                {
                    UpdateAllUsers();
                    return;
                }
            }

            if (this.ParamUserID > Null.NullInteger)
            {
                UpdateSingleUser();
                return;
            }

            if (this.ParamRoleID > Null.NullInteger)
            {
                UpdateUsersForRole();
                return;
            }

            if (this.ParamPortalID > Null.NullInteger)
            {
                UpdateAllUsersByPortal();
            }
        }

        public void UpdateAllUsers()
        {

            var sql = QueryAllUsers;

            using (var dataContext = DataContext.Instance())
            {
                dataContext.Execute(CommandType.Text, sql);
            }
        }

        public void UpdateAllUsersByPortal()
        {
            var sql = QueryUsersByPortal;

            using (var dataContext = DataContext.Instance())
            {
                dataContext.Execute(CommandType.Text, sql, new { PortalID = this.ParamPortalID });
            }
        }

        public void UpdateSingleUser()
        {

            var sql = QuerySingleUser;

            using (var dataContext = DataContext.Instance())
            {
                dataContext.Execute(CommandType.Text, sql, new { UserID = this.ParamUserID });
            }
        }

        public void UpdateUsersForRole()
        {
            var sql = QueryUsersByRole;
            
            using (var dataContext = DataContext.Instance())
            {
                dataContext.Execute(CommandType.Text, sql, new { RoleID = this.ParamRoleID });
            }
        }

        public int GetPortalIdForRole()
        {
            var sql = QueryGetRolePortalID;

            using (var dataContext = DataContext.Instance())
            {
                return dataContext.ExecuteSingleOrDefault<int>(
                    CommandType.Text,
                    sql,
                    new { RoleId = this.ParamRoleID } // Objeto anónimo con el parámetro
                );
            }
        }
        private bool ValidateFlags()
        {
            if (!string.IsNullOrEmpty(ParamScope))
            {
                // Scope can only be "all" or "current"
                if (ParamScope != ParamAll && ParamScope != ParamCurrent)
                {
                    return false;
                }

                // If Scope is populated, return false if any of the other variables have any value above -1
                if (ParamUserID > -1 || ParamRoleID > -1 || ParamPortalID > -1)
                {
                    return false;
                }

                return true;
            }
            else
            {
                // If Scope is an empty string, return true based on specific conditions
                if ((ParamUserID > -1 && ParamRoleID == -1 && ParamPortalID == -1) ||
                    (ParamRoleID > -1 && ParamUserID == -1 && ParamPortalID == -1) ||
                    (ParamPortalID > -1 && ParamUserID == -1 && ParamRoleID == -1))
                {
                    return true;
                }
            }

            // Return false for all other cases
            return false;
        }

        protected override void LogError(Exception ex)
        {
            if (ex != null)
            {
                Logger.Error(ex.Message, ex);
                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message, ex.InnerException);
                }
            }
        }
        #endregion
    }
}