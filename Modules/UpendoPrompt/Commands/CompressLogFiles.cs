using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Web;
using DotNetNuke.Instrumentation;
using Upendo.Modules.UpendoPrompt.Components;
using Constants = Upendo.Modules.UpendoPrompt.Components.Constants;
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Upendo.Modules.UpendoPrompt.Entities;
using System.Web.Security;
using DotNetNuke.UI.UserControls;
using DotNetNuke.Services.Scheduling;
using Upendo.Modules.UpendoPrompt.ScheduledJobs.ClearLogsJob;
using Upendo.Modules.UpendoPrompt.Utility;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("compress-log-files", Constants.PromptCategory, "CompressLogFiles")]
    public class CompressLogFiles : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CompressLogFiles));

        #region Properties
        #endregion

        #region Implementation

        public override ConsoleResultModel Run()
        {
            try
            {
                RunClearLogsJob();
                var output = (string.Format(this.LocalizeString(Constants.LocalizationKeys.CompressLogFilesRunnerSuccess)));
                return new ConsoleResultModel
                {
                    Output = output,
                    Data = new List<PromptMessage>(),
                    IsError = false
                };
            }
            catch (Exception e)
            {
                LogError(e);
                return new ConsoleErrorResultModel(string.Concat(Constants.OutputPrefix, this.LocalizeString(Constants.LocalizationKeys.CompressLogFilesRunnerFailure)));
            }
        }

        private void RunClearLogsJob()
        {
            var schedulerItem = new ScheduleHistoryItem();
            var clearLogsJob = new ClearLogsJob(schedulerItem);
            clearLogsJob.DoWork();
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
