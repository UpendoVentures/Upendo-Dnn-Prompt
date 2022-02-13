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

namespace Upendo.Modules.UpendoPrompt.Components
{
    public static class Constants
    {
        public const string PromptLocalResourceFile = "~/DesktopModules/UpendoPrompt/App_LocalResources/Global.resx";
        public const string Namespace = "uvm";
        public const string PromptCategory = "Upendo Ventures";
        public const string OutputPrefix = "Upendo Prompt";

        public const string STATE_TRUE = "true";
        public const string STATE_FALSE = "false";

        public static class SettingKeys
        {
            public const string HostSetting_DebugMode = "DebugMode";
            public const string PortalSetting_Popup = "EnablePopUps";
        }

        public static class LocalizationKeys
        {
            public const string PopupsEnabled = "PopupEnabled";
            public const string PopupDisabled = "PopupDisabled";
            public const string ErrorOccurred = "ErrorOccurred";

            public const string FlagMode = "Prompt_PopupMode_FlagMode";
            public const string PromptModeInvalid = "Prompt_ModeInvalid";

            public const string FlagScope = "Prompt_PopupMode_FlagScope";
            public const string PromptScopeInvalid = "Prompt_ScopeInvalid";
            public const string ScopeCurrent = "ScopeCurrent";
            public const string ScopeAll = "ScopeAll";

            public const string DebugOn = "DebugOn";
            public const string DebugOff = "DebugOff";
        }
    }
}