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

        private const string GenericAuthValue = "Dnn2002!";

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

                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Tony", LastName = "Stark", Email = "edith@example.com", Username = "tstark"
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Diana", LastName = "Prince", Email = "dembootstho@example.com", Username = "dprince"
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Peter", LastName = "Quill", Email = "gamoralover@example.com", Username = "pquill"
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Rey", LastName = "Skywalker", Email = "sithlight@example.com", Username = "rskywalker"
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Ethan", LastName = "Hunt", Email = "possible@example.com", Username = "ehunt"
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Wade", LastName = "Wilson", Email = "newguy@example.com", Username = "wwilson"
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Natasha", LastName = "Romanov", Email = "boss@example.com", Username = "nromanov"
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Hermione", LastName = "Granger", Email = "wanded@example.com", Username = "hgranger"
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "John", LastName = "Wick", Email = "ripmrwick@example.com", Username = "jwick"
                }));
                messages.Add(CreateUserAccount(new DemoUser
                {
                    FirstName = "Eggsy", LastName = "Unwin", Email = "gentleman@example.com", Username = "eunwin"
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
            MembershipProvider provider = Membership.Provider;
            int minLength = provider.MinRequiredPasswordLength;

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
                    Password = minLength > 7 ? GetPassword(minLength) : GenericAuthValue
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

                Logger.Debug($"User Creation Status for '{user.DisplayName} ({user.Email})' was {status}."); // just in case we need to troubleshoot something 

                return new PromptMessage(string.Format(this.LocalizeString(Constants.LocalizationKeys.CreateUserSuccess), user.DisplayName, user.Email));
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

        private static string GeneratePassword(int minLength)
        {
            string basePassword = GenericAuthValue;
            string passwordWithoutSymbol = basePassword.Split('!')[0];
            int baseLength = passwordWithoutSymbol.Length + 1; 

            if (minLength <= baseLength)
            {
                return basePassword.Substring(0, minLength);
            }

            int additionalLength = minLength - baseLength;
            string additionalDigits = GenerateSequentialDigits(additionalLength);

            return passwordWithoutSymbol + additionalDigits + "!";
        }

        private static string GenerateSequentialDigits(int length)
        {
            StringBuilder sb = new StringBuilder();
            int currentDigit = 1;

            for (int i = 0; i < length; i++)
            {
                sb.Append(currentDigit);
                currentDigit++;

                if (currentDigit == 10)
                {
                    currentDigit = 1;
                }
            }

            return sb.ToString();
        }
        public static string GetPassword(int minRequiredPasswordLength)
        {
            return GeneratePassword(minRequiredPasswordLength);
        }
        #endregion
    }
}