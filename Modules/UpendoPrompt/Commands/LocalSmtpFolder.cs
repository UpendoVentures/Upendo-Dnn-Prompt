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
using System.Security;
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
    [ConsoleCommand("set-localsmtpfolder", Constants.PromptCategory, "PromptLocalSmtpFolder")]
    public class LocalSmtpFolder : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LocalSmtpFolder));

        #region Constants

        private const string LOCAL_SMTP_FOLDER_NAME = "_SMTP";
        private const string LOCAL_SMTP_ENSURE_SYSTEM_NET = "~/DesktopModules/UpendoPrompt/Config/LocalSmtpFolder.EnsureSystemNet.xml.resources";
        private const string LOCAL_SMTP_INSTALL = "~/DesktopModules/UpendoPrompt/Config/LocalSmtpFolder.Install.xml.resources";
        private const string LOCAL_SMTP_RESTORE = "~/DesktopModules/UpendoPrompt/Config/LocalSmtpFolder.Restore.xml.resources";
        private const string PICKUP_DIRECTORY_TOKEN = "__PICKUP_DIRECTORY__";
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
                var localSmtpEnabled = LocalSmtpFolderIsConfigured();

                if (localSmtpEnabled)
                {
                    var restoreFilePath = HttpContext.Current.Server.MapPath(LOCAL_SMTP_RESTORE);
                    ExecuteMerge(restoreFilePath);

                    DataCache.ClearCache();

                    return new CustomConsoleResultModel
                    {
                        Output = this.LocalizeString(Constants.LocalizationKeys.LocalSmtpFolderRestored),
                        IsError = false
                    };
                }

                var pickupDirectory = EnsureLocalSmtpFolder();
                var installFilePath = CreateInstallMergeFile(pickupDirectory);

                try
                {
                    EnsureSystemNetNode();
                    ExecuteMerge(installFilePath);
                }
                finally
                {
                    DeleteTemporaryMergeFile(installFilePath);
                }

                DataCache.ClearCache();

                return new CustomConsoleResultModel
                {
                    Output = string.Format(this.LocalizeString(Constants.LocalizationKeys.LocalSmtpFolderEnabled), pickupDirectory),
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

        private static string CreateInstallMergeFile(string pickupDirectory)
        {
            var installFilePath = HttpContext.Current.Server.MapPath(LOCAL_SMTP_INSTALL);
            var installTemplate = File.ReadAllText(installFilePath);
            var escapedPickupDirectory = SecurityElement.Escape(pickupDirectory);
            var mergeContent = installTemplate.Replace(PICKUP_DIRECTORY_TOKEN, escapedPickupDirectory);
            var tempFilePath = Path.Combine(Path.GetTempPath(), string.Concat("UpendoPrompt.LocalSmtpFolder.", Guid.NewGuid().ToString("N"), ".xml"));

            File.WriteAllText(tempFilePath, mergeContent);

            return tempFilePath;
        }

        private static string EnsureLocalSmtpFolder()
        {
            var pickupDirectory = Path.Combine(Globals.ApplicationMapPath, LOCAL_SMTP_FOLDER_NAME);
            Directory.CreateDirectory(pickupDirectory);

            return pickupDirectory;
        }

        private static void EnsureSystemNetNode()
        {
            if (SystemNetNodeExists())
            {
                return;
            }

            var ensureSystemNetFilePath = HttpContext.Current.Server.MapPath(LOCAL_SMTP_ENSURE_SYSTEM_NET);
            ExecuteMerge(ensureSystemNetFilePath);
        }

        private static void DeleteTemporaryMergeFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static void ExecuteMerge(string xmlDoc)
        {
            var app = DotNetNukeContext.Current.Application;
            var merge = new DotNetNuke.Services.Installer.XmlMerge(xmlDoc, Globals.FormatVersion(app.Version), app.Description);
            merge.UpdateConfigs();
        }

        private static bool LocalSmtpFolderIsConfigured()
        {
            var doc = LoadWebConfig();
            var smtpNode = doc.SelectSingleNode("/configuration/system.net/mailSettings/smtp");
            var pickupDirectoryNode = doc.SelectSingleNode("/configuration/system.net/mailSettings/smtp/specifiedPickupDirectory");

            if (smtpNode == null || smtpNode.Attributes == null || pickupDirectoryNode == null)
            {
                return false;
            }

            var deliveryMethod = smtpNode.Attributes["deliveryMethod"] == null ? string.Empty : smtpNode.Attributes["deliveryMethod"].Value;

            return deliveryMethod.Equals("SpecifiedPickupDirectory", StringComparison.OrdinalIgnoreCase);
        }

        private static XmlDocument LoadWebConfig()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WEB_CONFIG);
            var doc = new XmlDocument();
            doc.Load(configPath);

            return doc;
        }

        private static bool SystemNetNodeExists()
        {
            var doc = LoadWebConfig();

            return doc.SelectSingleNode("/configuration/system.net") != null;
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
