
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Xml;

namespace Upendo.Modules.UpendoPrompt.Components
{
    public sealed class FeatureController: IUpgradeable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FeatureController));
        private const string upendoPromptLogInfo = "Upendo Prompt - Upgrade Module...";
        private const string actionSuccess = "Success";
        private const string actionFailed = "Failed";
        private const string friendlyNameClearDnnLogs = "Clear DNN logs";
        private const string typeFullNameClearDnnLogs = "Upendo.Modules.UpendoPrompt.ScheduledJobs.ClearLogsJob, Upendo.Modules.UpendoPrompt";

        public string UpgradeModule(string Version)
        {
            try
            {
                Logger.Info(upendoPromptLogInfo);

                switch (Version)
                {
                    case "01.09.00":
                        Logger.Info(Constants.LocalizationKeys.CreatingClearLogsScheduler);
                        CreateClearLogsScheduler();
                        break;

                    default:
                        break;
                }

                return actionSuccess;
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                Exceptions.LogException(ex);

                return actionFailed;
            }
        }

        private void LogError(string message, Exception ex)
        {
            if (ex != null)
            {
                Logger.Error(message, ex);
                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message, ex.InnerException);
                }
            }
            else
            {
                Logger.Error(message);
            }
        }



        public void CreateClearLogsScheduler()
        {
            var friendlyName = friendlyNameClearDnnLogs;
            var typeFullName = typeFullNameClearDnnLogs;

            var schedules = SchedulingProvider.Instance().GetSchedule().OfType<ScheduleItem>();
            if (!schedules.Any(s => s.FriendlyName == friendlyName))
            {
                var oItem = new ScheduleItem
                {
                    FriendlyName = friendlyName,
                    TypeFullName = typeFullName,
                    Enabled = true,
                    CatchUpEnabled = false,
                    RetainHistoryNum = 0,
                    TimeLapse = 1,
                    TimeLapseMeasurement = "d",
                    RetryTimeLapse = 1,
                    RetryTimeLapseMeasurement = "d",
                    ScheduleSource = ScheduleSource.NOT_SET
                };
                SchedulingProvider.Instance().AddSchedule(oItem);
            }
        }
    }
}
