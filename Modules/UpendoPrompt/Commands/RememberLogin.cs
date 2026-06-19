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
using System.IO;
using System.Web;
using System.Xml;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using Upendo.Modules.UpendoPrompt.Components;
using Upendo.Modules.UpendoPrompt.Custom;
using Constants = Upendo.Modules.UpendoPrompt.Components.Constants;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("set-rememberlogin", Constants.PromptCategory, "PromptRememberLogin")]
    public class RememberLogin : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RememberLogin));

        #region Constants

        private const string PERSISTENT_COOKIE_TIMEOUT = "PersistentCookieTimeout";
        private const string REMEMBER_LOGIN_INSTALL = "~/DesktopModules/UpendoPrompt/Config/RememberLogin.Install.xml.resources";
        private const string REMEMBER_LOGIN_RESTORE = "~/DesktopModules/UpendoPrompt/Config/RememberLogin.Restore.xml.resources";
        private const string REMEMBER_LOGIN_TIMEOUT = "120";
        private const string REMEMBER_LOGIN_PERSISTENT_COOKIE_TIMEOUT = "20160";
        private const string WEB_CONFIG = "web.config";

        #endregion

        #region Implementation

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            // do nothing
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var rememberLoginEnabled = RememberLoginIsConfigured();
                var mergeFile = rememberLoginEnabled ? REMEMBER_LOGIN_RESTORE : REMEMBER_LOGIN_INSTALL;
                var filePath = HttpContext.Current.Server.MapPath(mergeFile);

                ExecuteMerge(filePath);

                DataCache.ClearCache();

                return new CustomConsoleResultModel
                {
                    Output = rememberLoginEnabled
                        ? this.LocalizeString(Constants.LocalizationKeys.RememberLoginRestored)
                        : this.LocalizeString(Constants.LocalizationKeys.RememberLoginEnabled),
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

        private static void ExecuteMerge(string xmlDoc)
        {
            var app = DotNetNukeContext.Current.Application;
            var merge = new DotNetNuke.Services.Installer.XmlMerge(xmlDoc, Globals.FormatVersion(app.Version), app.Description);
            merge.UpdateConfigs();
        }

        private static string GetAttributeValue(XmlDocument doc, string xpath, string attributeName)
        {
            var node = doc.SelectSingleNode(xpath);

            if (node == null || node.Attributes == null || node.Attributes[attributeName] == null)
            {
                return string.Empty;
            }

            return node.Attributes[attributeName].Value;
        }

        private static string GetPersistentCookieTimeout(XmlDocument doc)
        {
            var node = doc.SelectSingleNode("/configuration/appSettings/add[@key='" + PERSISTENT_COOKIE_TIMEOUT + "']");

            if (node == null || node.Attributes == null || node.Attributes["value"] == null)
            {
                return string.Empty;
            }

            return node.Attributes["value"].Value;
        }

        private static bool RememberLoginIsConfigured()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WEB_CONFIG);
            var doc = new XmlDocument();
            doc.Load(configPath);

            var formsTimeout = GetAttributeValue(doc, "/configuration/system.web/authentication/forms", "timeout");
            var persistentCookieTimeout = GetPersistentCookieTimeout(doc);

            return formsTimeout.Equals(REMEMBER_LOGIN_TIMEOUT, StringComparison.OrdinalIgnoreCase)
                && persistentCookieTimeout.Equals(REMEMBER_LOGIN_PERSISTENT_COOKIE_TIMEOUT, StringComparison.OrdinalIgnoreCase);
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
