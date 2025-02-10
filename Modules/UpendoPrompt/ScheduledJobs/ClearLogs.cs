using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Scheduling;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;

namespace Upendo.Modules.UpendoPrompt.ScheduledJobs.ClearLogsJob
{
    public class ClearLogsJob : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ClearLogsJob));
        private const string upendoPromptDirectory = "Portals\\_default\\logs";
        public ClearLogsJob(ScheduleHistoryItem oItem)

            : base()

        {

            this.ScheduleHistoryItem = oItem;

        }
        public override void DoWork()
        {
            try
            {
                this.Progressing();

                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                string logsDirectory = Path.GetFullPath(Path.Combine(baseDirectory, upendoPromptDirectory));

                DateTime CurrentDate = DateTime.Now;

                int SearchMonth = CurrentDate.Month - 1;

                DirectoryInfo directoryInfo = new DirectoryInfo(logsDirectory);
                List<FileInfo> logs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Where(l => l.CreationTime.Month == SearchMonth).ToList();

                if (logs.Count > 0)
                {
                    string directoryToZip = $"{logsDirectory}/Logs-{new DateTime(CurrentDate.Year, SearchMonth, 1):yyyyMM}";
                    Directory.CreateDirectory(directoryToZip);

                    foreach (FileInfo log in logs)
                    {

                        string destinationPath = Path.Combine(directoryToZip, log.Name);

                        File.Move(log.FullName, destinationPath);
                    }

                    ZipFile.CreateFromDirectory(directoryToZip, $"{directoryToZip}.zip", CompressionLevel.Optimal, true);

                    Directory.Delete(directoryToZip, true);
                }


                //Show success

                this.ScheduleHistoryItem.Succeeded = true;

            }
            catch (Exception ex)
            {
                Logger.Error(JsonConvert.SerializeObject(ex.Message));

                this.ScheduleHistoryItem.Succeeded = false;

                this.Errored(ref ex);

                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);

            }

        }

    }
}