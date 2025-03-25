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
using DotNetNuke.Common.Utilities; // added for the commented-out clearing of cache below
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Log.EventLog;
using Upendo.Modules.UpendoPrompt.Custom;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("impersonate", Constants.PromptCategory, "Impersonate")]
    public class Impersonate : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Impersonate));

        #region Constants

        #endregion

        #region Properties

        [FlagParameter("username", Constants.LocalizationKeys.FlagUsername, "String", "", false)]
        private const string FlagUsername = "username";
        private string Username { get; set; }

        [FlagParameter("userid", Constants.LocalizationKeys.FlagUserId, "Integer", "-1", false)]
        private const string FlagUserId = "userid";
        private int UserID { get; set; }

        private UserInfo impersonateUser { get; set; }
        private UserInfo currentUser { get; set; }

        #endregion

        #region Implementation

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            // get the command flags, if present
            this.Username = this.GetFlagValue(FlagUsername, "username", string.Empty, false);
            this.UserID = this.GetFlagValue(FlagUserId, "userid", Null.NullInteger, false);

            // perform validation

            if (string.IsNullOrEmpty(this.Username) && this.UserID == Null.NullInteger)
            {
                this.AddMessage(this.LocalizeString(Constants.LocalizationKeys.ImpersonateFlagsInvalid));
            }

            if (!string.IsNullOrEmpty(this.Username) && this.UserID > Null.NullInteger)
            {
                this.AddMessage(this.LocalizeString(Constants.LocalizationKeys.ImpersonateFlagsInvalid));
            }
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                // verfiy the parameters again 
                UserInfo user = null;
                if (this.UserID > Null.NullInteger)
                {
                    user = DotNetNuke.Entities.Users.UserController.Instance.GetUserById(this.PortalId, this.UserID);
                }
                else if (!string.IsNullOrEmpty(this.Username))
                {
                    user = DotNetNuke.Entities.Users.UserController.GetUserByName(this.PortalId, this.Username);
                }

                this.impersonateUser = user;
                this.currentUser = this.User;

                var output = string.Empty;
                if (user == null || user.UserID == Null.NullInteger)
                {
                    // no user was found 
                    output = this.LocalizeString(Constants.LocalizationKeys.ImpersonateNoUserFound);

                    return new CustomConsoleResultModel
                    {
                        Output = output,
                        IsError = true
                    };
                }

                // verify the current user is authorized to do this  
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                if (currentUser == null || currentUser.UserID == -1 || !currentUser.IsSuperUser)
                {
                    output = this.LocalizeString(Constants.LocalizationKeys.ImpersonateNotAuthorized);

                    return new CustomConsoleResultModel
                    {
                        Output = output,
                        IsError = true
                    };
                }

                ImpersonateUserAccount();

                output = this.LocalizeString(Constants.LocalizationKeys.ImpersonateSuccess);

                return new CustomConsoleResultModel
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

        private void ImpersonateUserAccount()
        {
            //
            //      Impersonate the given user
            //

            try
            {
                //Log event
                var log = new LogInfo
                {
                    BypassBuffering = true,
                    LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString(),
                };
                log.AddProperty("Message", string.Format(this.LocalizeString("Prompt_UserImpersonated"), this.currentUser.Username, this.impersonateUser.Username));
                LogController.Instance.AddLog(log);

                //Remove user from cache
                DataCache.ClearUserCache(PortalSettings.PortalId, this.currentUser.Username);

                var objPortalSecurity = new PortalSecurity();
                objPortalSecurity.SignOut();

                var ip = this.currentUser.LastIPAddress;

                UserController.UserLogin(this.impersonateUser.PortalID, this.impersonateUser, PortalSettings.PortalName, ip, false);

                var homeTabId = PortalSettings.HomeTabId;
                var homeTab = DotNetNuke.Entities.Tabs.TabController.Instance.GetTab(homeTabId, PortalSettings.PortalId);

                // will not redirect and only logs an error 
                //HttpContext.Current.Response.Redirect(homeTab.FullUrl, true);
            }
            catch (Exception e)
            {
                LogError(e);
                throw;
            }
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