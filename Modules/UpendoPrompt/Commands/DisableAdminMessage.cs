using System;
using System.Collections.Generic;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Upendo.Modules.UpendoPrompt.Components;
using System.Linq;
using Upendo.Modules.UpendoPrompt.Custom;
using DotNetNuke.Common.Utilities;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("disable-adminmessage",Constants.PromptCategory, "DisableAdminMessage")]
    public class DisableAdminMessage : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DisableAdminMessage));

        public override ConsoleResultModel Run()
        {
            try
            {
                DataCache.ClearCache();

                // Get the list of all modules in the portal
                var modules = ModuleController.Instance.GetModules(this.PortalId);
                int updatedCount = 0;

                foreach (ModuleInfo module in modules)
                {
                    if (module != null && module.TabModuleID > 0)
                    {                       
                        // Retrieve the current value of the "hideadminborder" setting for the module
                        var currentSetting = module.TabModuleSettings.ContainsKey("hideadminborder")
                            ? module.TabModuleSettings["hideadminborder"]
                            : null;

                        // Check if the setting is not set or is not "True"
                        if (currentSetting == null || !currentSetting.ToString().Equals("True", StringComparison.OrdinalIgnoreCase))
                        {
                            updatedCount++;
                            // Update or create the "hideadminborder" setting with the value "True"
                            ModuleController.Instance.UpdateTabModuleSetting(module.TabModuleID, "hideadminborder", "True");
                        }
                    }
                }
                var output = string.Format(this.LocalizeString(Constants.LocalizationKeys.DisableAdminMessageResult), updatedCount);
                return new CustomConsoleResultModel
                {
                    Output = output,
                    IsError = false
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(string.Concat(Constants.OutputPrefix, this.LocalizeString(Constants.LocalizationKeys.ErrorOccurred)));
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
    }
}