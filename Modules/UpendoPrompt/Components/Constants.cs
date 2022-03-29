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
        
        public const string CONFIG_EXT = ".config";
        public const string LOG4NET_CONFIG = "DotNetNuke.log4net.config";

        public const string STATE_TRUE = "true";
        public const string STATE_FALSE = "false";

        public const string FORMAT_IMPORTANT = "<span style=\"color:red;font-weight:bold;>{0}</span>";
        public const string FORMAT_WARNING = "<span style=\"color:yellow;font-weight:bold;>{0}</span>";
        public const string FORMAT_SUCCESS = "<span style=\"color:green;font-weight:bold;>{0}</span>";

        public const string FOLDER_EXTENSIONPACKAGES = "*.resources";

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

            public const string DebugStatus_Host = "DebugStatusHost";
            public const string DebugStatus_Log4net = "DebugStatusLog4net";
            public const string DebugStatus_WebConfig = "DebugStatusWebConfig";
            public const string DebugEnabled = "DebugEnabled";
            public const string DebugDisabled = "DebugDisabled";

            public const string ENABLED = "Enabled";
            public const string DISABLED = "Disabled";

            public const string RECORDS_NONE = "RecordsNone";
            public const string RECORDS_SOME = "RecordsSome";

            public const string DEPRECATED= "Deprecated";

            public const string FlagUsername = "Prompt_Impersonate_FlagUsername";
            public const string FlagUserId = "Prompt_Impersonate_FlagUserID";

            public const string ImpersonateFlagsInvalid = "Prompt_ImpersonateFlagsInvalid";
            public const string ImpersonateNoUserFound = "Prompt_ImpersonateNoUserFound";
            public const string ImpersonateNotAuthorized = "Prompt_ImpersonateNotAuthorized";
            public const string ImpersonateSuccess = "Prompt_ImpersonateSuccess";

            public const string PackageDeletionErrorFormat = "PackageDeletionErrorFormat";
            public const string PackageDeletionSuccess = "PackageDeletionSuccess";

            public const string FileSizeMessage = "FileSizeMessage";
        }

        public static class Procedures
        {
            public const string ThemesUsed = "uvm_Prompt_GetThemesUsed";
        }
    }
}