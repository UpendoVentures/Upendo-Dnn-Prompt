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
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Common.Utilities; // added for the commented-out clearing of cache below
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Upendo.Modules.UpendoPrompt.Commands.Deprecated
{
    [Obsolete("Please use 'set-popups' instead. Will be removed in version 1.5.0 or higher.")]
    [ConsoleCommand("popup-mode", Constants.PromptCategory, "PromptPopupMode")]
    public class PopupMode : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PopupMode));

        #region Constants

        private const string SCOPE_NAME = "Scope";
        private const string SCOPE_ALL = "all";
        private const string SCOPE_CURRENT = "current";

        private const string MODE_NAME = "Mode";
        private const string MODE_DISABLE = "disable";
        private const string MODE_ENABLE = "enable";

        #endregion

        #region Properties

        [FlagParameter("scope", Constants.LocalizationKeys.FlagScope, "String", SCOPE_CURRENT, false)]
        private const string FlagScope = "scope";
        private string Scope { get; set; }

        [FlagParameter("mode", Constants.LocalizationKeys.FlagMode, "String", MODE_DISABLE, false)]
        private const string FlagMode = "mode";
        private string Mode { get; set; }

        #endregion

        #region Implementation

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            // get the command flags, if present
            this.Scope = this.GetFlagValue(FlagScope, SCOPE_NAME, SCOPE_CURRENT, false);
            this.Mode = this.GetFlagValue(FlagMode, MODE_NAME, MODE_DISABLE, false);

            // perform validation

            if (!string.IsNullOrEmpty(this.Mode) && (Mode != MODE_DISABLE && Mode != MODE_ENABLE))
            {
                this.AddMessage(this.LocalizeString(Constants.LocalizationKeys.PromptModeInvalid));
            }

            if (!string.IsNullOrEmpty(this.Scope) && (Scope != SCOPE_CURRENT && Scope != SCOPE_ALL))
            {
                this.AddMessage(this.LocalizeString(Constants.LocalizationKeys.PromptScopeInvalid));
            }
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var output = string.Format(LocalizeString(Constants.LocalizationKeys.DEPRECATED), "set-popups");

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