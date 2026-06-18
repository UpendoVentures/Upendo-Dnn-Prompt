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
