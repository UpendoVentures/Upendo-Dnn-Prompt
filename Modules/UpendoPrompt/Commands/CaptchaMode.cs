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
using DotNetNuke.Instrumentation;
using Upendo.Modules.UpendoPrompt.Components;
using Constants = Upendo.Modules.UpendoPrompt.Components.Constants;

// required for DNN Prompt
//using DotNetNuke.Prompt; // deprecated Prompt is replaced with this
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("set-captcha", Constants.PromptCategory, "PromptCaptchaMode")]
    public class CaptchaMode : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CaptchaMode));
        
        #region Implementation

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            // no custom switches to wire up 
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var strUseCaptcha = PortalController.GetPortalSetting(Constants.SettingKeys.PortalSettings_UseCaptcha, this.PortalId, Constants.STATE_FALSE);
                var blnEnabled = false; 

                if (!string.IsNullOrEmpty(strUseCaptcha) && strUseCaptcha != Constants.STATE_FALSE)
                {
                    // the setting was found and it's not "false"
                    // treating as true
                    PortalController.Instance.UpdatePortalSetting(
                        PortalSettings.PortalId,
                        Constants.SettingKeys.PortalSettings_UseCaptcha,
                        Constants.STATE_FALSE, // toggled to false 
                        true,
                        PortalSettings.CultureCode,
                        false);
                }
                else
                {
                    // the setting was not found or it is false 
                    // treating as false 
                    PortalController.Instance.UpdatePortalSetting(
                        PortalSettings.PortalId,
                        Constants.SettingKeys.PortalSettings_UseCaptcha,
                        Constants.STATE_TRUE, // toggled to true 
                        true, 
                        PortalSettings.CultureCode,
                        false);

                    blnEnabled = true;
                }

                var output = string.Empty;

                if (blnEnabled)
                {
                    output = string.Format(this.LocalizeString(Constants.LocalizationKeys.CaptchaResult), LocalizeString(Constants.LocalizationKeys.State_On));
                }
                else
                {
                    output = string.Format(this.LocalizeString(Constants.LocalizationKeys.CaptchaResult), LocalizeString(Constants.LocalizationKeys.State_Off));
                }

                // clear DNN cache 
                DataCache.ClearCache();

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