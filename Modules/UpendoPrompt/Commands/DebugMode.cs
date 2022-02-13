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
using System.Xml;
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host; 
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using Upendo.Modules.UpendoPrompt.Components;
using Constants = Upendo.Modules.UpendoPrompt.Components.Constants;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("debug-mode", Constants.PromptCategory, "PromptDebugMode")]
    public class DebugMode : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DebugMode));

        #region Constants

        private const string WEBCONFIG_DEBUG_ON = "~/DesktopModules/UpendoPrompt/Config/webConfig-DebugOn.xml.resources";
        private const string WEBCONFIG_DEBUG_OFF = "~/DesktopModules/UpendoPrompt/Config/webConfig-DebugOff.xml.resources";
        
        private const string LOG4NET_DEBUG_ON = "~/DesktopModules/UpendoPrompt/Config/log4net-DebugOn.xml.resources";
        private const string LOG4NET_DEBUG_OFF = "~/DesktopModules/UpendoPrompt/Config/log4net-DebugOff.xml.resources";

        private const string CONFIG_EXT = ".config";

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
                var currentState = Host.DebugMode;
                var newState = !currentState;

                // site debug mode
                HostController.Instance.Update(Constants.SettingKeys.HostSetting_DebugMode, newState.ToString());

                // web.config debug mode
                MergeWebConfig(newState);
                
                // log4net debug mode
                MergeLog4net(newState);

                var output = string.Empty;

                if (newState)
                {
                    output = this.LocalizeString(Constants.LocalizationKeys.DebugOn);
                }
                else
                {
                    output = this.LocalizeString(Constants.LocalizationKeys.DebugOff);
                }
                
                return new ConsoleResultModel
                {
                    Output = output
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

        private void MergeWebConfig(bool newState)
        {
            var filePath = System.Web.HttpContext.Current.Server.MapPath(newState ? WEBCONFIG_DEBUG_ON : WEBCONFIG_DEBUG_OFF);

            ExecuteMerge(filePath);
        }

        private void MergeLog4net(bool newState)
        {
            var filePath = System.Web.HttpContext.Current.Server.MapPath(newState ? LOG4NET_DEBUG_ON : LOG4NET_DEBUG_OFF);

            ExecuteMerge(filePath);
        }

        public string GetConfigFile(string configFile)
        {
            if (configFile.EndsWith(CONFIG_EXT, StringComparison.InvariantCultureIgnoreCase))
            {
                var configDoc = Config.Load(configFile);
                using (var txtWriter = new StringWriter())
                {
                    using (var writer = new XmlTextWriter(txtWriter))
                    {
                        writer.Formatting = Formatting.Indented;
                        configDoc.WriteTo(writer);
                    }

                    return txtWriter.ToString();
                }
            }
            else
            {
                var doc = File.ReadAllText(Path.Combine(Globals.ApplicationMapPath, configFile));
                return doc;
            }
        }

        private void ExecuteMerge(string xmlDoc)
        {
            var app = DotNetNukeContext.Current.Application;
            var merge = new DotNetNuke.Services.Installer.XmlMerge(xmlDoc, Globals.FormatVersion(app.Version), app.Description);
            merge.UpdateConfigs();

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