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
using System.Linq;
using Upendo.Modules.UpendoPrompt.Custom;

namespace Upendo.Modules.UpendoPrompt.Commands
{
    [ConsoleCommand("compress-log-files", Constants.PromptCategory, "CompressLogFiles")]
    public class CompressLogFiles : PromptBase, IConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CompressLogFiles));

        private const string SchedulerFlag = "scheduler";

        #region Properties
        private string Scheduler { get; set; }
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            this.Scheduler = this.GetFlagValue<string>(SchedulerFlag, $"{SchedulerFlag}", Null.NullString, false);
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                string schedulerMessage = string.Empty;
                if (!string.IsNullOrEmpty(Scheduler))
                {
                    schedulerMessage = ManageScheduler(Scheduler);
                }

                RunClearLogsJob();
                var output = string.Format(this.LocalizeString(Constants.LocalizationKeys.CompressLogFilesRunnerSuccess));
                if (!string.IsNullOrEmpty(schedulerMessage))
                {
                    output += Environment.NewLine + schedulerMessage;
                }

                return new CustomConsoleResultModel
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

        private string ManageScheduler(string schedulerOption)
        {
            var scheduleController = SchedulingProvider.Instance();
            var scheduleItem = scheduleController.GetSchedule().OfType<ScheduleItem>()
                .FirstOrDefault(s => s.TypeFullName == "Upendo.Modules.UpendoPrompt.ScheduledJobs.ClearLogsJob, Upendo.Modules.UpendoPrompt");

            if (scheduleItem == null)
            {
                var featureController = new FeatureController();
                featureController.CreateClearLogsScheduler();
                scheduleItem = scheduleController.GetSchedule().OfType<ScheduleItem>()
                    .FirstOrDefault(s => s.TypeFullName == "Upendo.Modules.UpendoPrompt.ScheduledJobs.ClearLogsJob, Upendo.Modules.UpendoPrompt");
            }

            if (scheduleItem != null)
            {
                bool enable = schedulerOption.Equals("enable", StringComparison.OrdinalIgnoreCase);
                scheduleItem.Enabled = enable;
                scheduleController.UpdateSchedule(scheduleItem);
                return enable ? this.LocalizeString(Constants.LocalizationKeys.SchedulerEnabled) : this.LocalizeString(Constants.LocalizationKeys.SchedulerDisabled);
            }

            return this.LocalizeString(Constants.LocalizationKeys.SchedulerNotFound);
        }
        #endregion
    }
}
