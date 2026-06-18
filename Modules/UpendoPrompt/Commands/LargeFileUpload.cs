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
    [ConsoleCommand("set-largefileupload", Constants.PromptCategory, "PromptLargeFileUpload")]
    public class LargeFileUpload : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LargeFileUpload));

        #region Constants

        private const int LARGE_MAX_REQUEST_LENGTH = 153600;
        private const long LARGE_MAX_ALLOWED_CONTENT_LENGTH = 157286400;
        private const string LARGE_EXTENSION_UPLOAD_INSTALL = "~/DesktopModules/UpendoPrompt/Config/LargeExtensionUpload.Intall.xml.resources";
        private const string LARGE_EXTENSION_UPLOAD_RESTORE = "~/DesktopModules/UpendoPrompt/Config/LargeExtensionUpload.Restore.xml.resources";
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
                var largeFilesSupported = LargeFilesAreSupported();
                var mergeFile = largeFilesSupported ? LARGE_EXTENSION_UPLOAD_RESTORE : LARGE_EXTENSION_UPLOAD_INSTALL;
                var filePath = HttpContext.Current.Server.MapPath(mergeFile);

                ExecuteMerge(filePath);

                // clear DNN cache so follow-up checks see the current configuration values
                DataCache.ClearCache();

                return new CustomConsoleResultModel
                {
                    Output = largeFilesSupported
                        ? this.LocalizeString(Constants.LocalizationKeys.LargeFileUploadRestored)
                        : this.LocalizeString(Constants.LocalizationKeys.LargeFileUploadEnabled),
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

        private bool LargeFilesAreSupported()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WEB_CONFIG);
            var doc = new XmlDocument();
            doc.Load(configPath);

            var maxRequestLength = GetIntAttribute(doc, "/configuration/system.web/httpRuntime", "maxRequestLength");
            var maxAllowedContentLength = GetLongAttribute(doc, "/configuration/system.webServer/security/requestFiltering/requestLimits", "maxAllowedContentLength");

            return maxRequestLength >= LARGE_MAX_REQUEST_LENGTH && maxAllowedContentLength >= LARGE_MAX_ALLOWED_CONTENT_LENGTH;
        }

        private static int GetIntAttribute(XmlDocument doc, string xpath, string attributeName)
        {
            var value = GetAttributeValue(doc, xpath, attributeName);
            var result = 0;
            int.TryParse(value, out result);

            return result;
        }

        private static long GetLongAttribute(XmlDocument doc, string xpath, string attributeName)
        {
            var value = GetAttributeValue(doc, xpath, attributeName);
            long result = 0;
            long.TryParse(value, out result);

            return result;
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

        private static void ExecuteMerge(string xmlDoc)
        {
            var app = DotNetNukeContext.Current.Application;
            var merge = new DotNetNuke.Services.Installer.XmlMerge(xmlDoc, Globals.FormatVersion(app.Version), app.Description);
            merge.UpdateConfigs();
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
