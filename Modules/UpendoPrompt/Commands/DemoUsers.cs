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
using System.Web.Security;
using System.Text;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("set-demousers", Constants.PromptCategory, "DemoUsers")]
    public class DemoUsers : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DemoUsers));


        #region Implementation

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            // no paramters 
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                MembershipProvider provider = Membership.Provider;
                int minLength = provider.MinRequiredPasswordLength;

                string passwordGenerator = Utility.PasswordGenerator.GeneratePassword(minLength);

                var messages = new List<PromptMessage>();

                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Tony", LastName = "Stark", Email = "edith@example.com", Username = "tstark", Password = passwordGenerator
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Diana", LastName = "Prince", Email = "dembootstho@example.com", Username = "dprince", Password = passwordGenerator
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Peter", LastName = "Quill", Email = "gamoralover@example.com", Username = "pquill", Password = passwordGenerator
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Rey", LastName = "Skywalker", Email = "sithlight@example.com", Username = "rskywalker", Password = passwordGenerator
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Ethan", LastName = "Hunt", Email = "possible@example.com", Username = "ehunt", Password = passwordGenerator
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Wade", LastName = "Wilson", Email = "newguy@example.com", Username = "wwilson", Password = passwordGenerator
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Natasha", LastName = "Romanov", Email = "boss@example.com", Username = "nromanov", Password = passwordGenerator
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Hermione", LastName = "Granger", Email = "wanded@example.com", Username = "hgranger", Password = passwordGenerator
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "John", LastName = "Wick", Email = "ripmrwick@example.com", Username = "jwick", Password = passwordGenerator
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Eggsy", LastName = "Unwin", Email = "gentleman@example.com", Username = "eunwin", Password = passwordGenerator
                }));

                var output = this.LocalizeString(Constants.LocalizationKeys.DemoUsersAdded);

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

        private PromptMessage CreateUserAccount(DemoUser newUser)
        {
            var OUser = DotNetNuke.Entities.Users.UserController.GetUserByName(this.PortalId, newUser.Username);
            var blnExists = (OUser != null);
          
            var user = new UserInfo
            {
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Username = newUser.Username,
                DisplayName = $"{newUser.FirstName} {newUser.LastName}",
                IsSuperUser = false,
                PortalID = this.PortalId,
                Membership = new UserMembership()
                {
                    Password = newUser.Password
                }
            };

            if (blnExists)
            {
                // no need to create the user account - it already exists 
                return new PromptMessage(string.Format(this.LocalizeString(Constants.LocalizationKeys.CreateUserExists), user.DisplayName, user.Email));
            }

            try
            {
                // creates the user account 
                var status = UserController.CreateUser(ref user);

                Logger.Debug($"User Creation Status for '{user.DisplayName} ({user.Email}) (Password: {newUser.Password})' was {status}."); // just in case we need to troubleshoot something 

                return new PromptMessage(string.Format(this.LocalizeString(Constants.LocalizationKeys.CreateUserSuccess), user.DisplayName, user.Email, newUser.Password));
            }
            catch (Exception e)
            {
                LogError(e);
                return new PromptMessage(string.Format(this.LocalizeString(Constants.LocalizationKeys.CreateUserFailure), user.DisplayName, user.Email));
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