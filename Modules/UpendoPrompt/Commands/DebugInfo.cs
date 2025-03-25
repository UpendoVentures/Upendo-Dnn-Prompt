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
using System.Web;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Host; 
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using Upendo.Modules.UpendoPrompt.Components;
using Upendo.Modules.UpendoPrompt.Custom;
using Upendo.Modules.UpendoPrompt.Entities;
using Constants = Upendo.Modules.UpendoPrompt.Components.Constants;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("list-debug", Constants.PromptCategory, "PromptDebugInfo")]
    public class DebugInfo : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DebugInfo));
        
        #region Implementation

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            // do nothing
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var debugHost = Host.DebugMode;
                var debugWebConfig = HttpContext.Current.IsDebuggingEnabled;
                var debugLog4net = GetLog4netStatus(); 

                var enabled = LocalizeString(Constants.LocalizationKeys.ENABLED).ToUpper();
                // DNN is stripping out comments with HTML added 
                //var enabled = string.Format(Constants.FORMAT_IMPORTANT, LocalizeString(Constants.LocalizationKeys.ENABLED));
                var disabled = LocalizeString(Constants.LocalizationKeys.DISABLED).ToUpper();

                var messages = new List<PromptMessage>();

                messages.Add(new PromptMessage
                {
                    Message = string.Format(LocalizeString(Constants.LocalizationKeys.DebugStatus_Host), debugHost ? enabled : disabled)
                });

                messages.Add(new PromptMessage
                {
                    Message = string.Format(LocalizeString(Constants.LocalizationKeys.DebugStatus_WebConfig), debugWebConfig ? enabled : disabled)
                });

                messages.Add(new PromptMessage
                {
                    Message = string.Format(LocalizeString(Constants.LocalizationKeys.DebugStatus_Log4net), debugLog4net ? enabled : disabled)
                });

                var output = string.Empty;
                if (debugWebConfig || debugLog4net || debugHost)
                {
                    output = LocalizeString(Constants.LocalizationKeys.DebugEnabled);
                    // DNN is stripping out comments with HTML added 
                    //output = string.Format(Constants.FORMAT_IMPORTANT, LocalizeString(Constants.LocalizationKeys.DebugEnabled));
                }
                else
                {
                    output = LocalizeString(Constants.LocalizationKeys.DebugDisabled);
                }

                return new CustomConsoleResultModel
                { 
                    Records = messages.Count,
                    Data = messages,
                    IsError = false, 
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