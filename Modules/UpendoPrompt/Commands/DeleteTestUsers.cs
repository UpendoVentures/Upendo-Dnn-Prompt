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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI;
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
using Upendo.Modules.UpendoPrompt.Entities;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("delete-testusers", Constants.PromptCategory, "DeleteTestUsers")]
    public class DeleteTestUsers : PromptBase, IConsoleCommand
    {
        private const string GenericUserNameValue = "testuser";
        private const string GenericEmailDomainValue = "@example.com";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DeleteTestUsers));

        #region Implementation

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            // no paramters 
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var messages = new List<PromptMessage>();
                var users = DotNetNuke.Entities.Users.UserController.GetUsers(this.PortalId);
                foreach (UserInfo item in users)
                {
                   
                    if (item.Email.Contains(GenericUserNameValue) && item.Email.Contains(GenericEmailDomainValue))
                    {
                        messages.Add(DeleteUserAccount(new TestUser
                        {
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Email = item.Email,
                            Username = item.Username
                        }));
                    }
                }

                // emp

                var output = this.LocalizeString(Constants.LocalizationKeys.TestUsersDeleted);

                // clear DNN cache 
                //DataCache.ClearCache();

                return new ConsoleResultModel
                {
                    Output = output,
                    Data = messages,
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

        private PromptMessage DeleteUserAccount(TestUser newUser)
        {
            var OUser = DotNetNuke.Entities.Users.UserController.GetUserByName(this.PortalId, newUser.Username);
            var blnExists = (OUser != null);

            if (blnExists)
            {
                // delete the user account  
                DotNetNuke.Entities.Users.UserController.DeleteUser(ref OUser, false, false);
                DotNetNuke.Entities.Users.UserController.RemoveUser(OUser);
                return new PromptMessage(string.Format(this.LocalizeString(Constants.LocalizationKeys.DeleteUserSuccess), newUser.FirstName, newUser.LastName, newUser.Email));
            }

            // message about the user not needing to be deleted  
            return new PromptMessage(string.Format(this.LocalizeString(Constants.LocalizationKeys.DeleteUserNotNeeded), newUser.FirstName, newUser.LastName, newUser.Email));
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