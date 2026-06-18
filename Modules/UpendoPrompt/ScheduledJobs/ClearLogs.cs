#region License

// Distributed under the MIT License
// ============================================================
// Copyright (c) Upendo Ventures, LLC.
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

namespace Upendo.Modules.UpendoPrompt.ScheduledJobs
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text.RegularExpressions;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Scheduling;
    using Newtonsoft.Json;

    public class ClearLogsJob : SchedulerClient
    {
        private const string UpendoPromptDirectory = "Portals\\_default\\logs";
        private const string StandardLogDateFormat = "yyyy.MM.dd";
        private const string InstallerLogDateFormat = "yyyyMMdd";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ClearLogsJob));

        private static readonly Regex StandardLogFileRegex = new Regex(
            @"^(?<date>\d{4}\.\d{2}\.\d{2})\.log\.resources$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex InstallerLogFileRegex = new Regex(
            @"^InstallerLog(?<date>\d{8})\.resources$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ClearLogsJob(ScheduleHistoryItem oItem)
            : base()
        {
            this.ScheduleHistoryItem = oItem;
        }

        public LogCompressionResult Result { get; private set; }

        public override void DoWork()
        {
            try
            {
                this.Progressing();

                this.Result = this.CompressPreviousMonthLogs();

                this.ScheduleHistoryItem.Succeeded = true;

                Logger.Info(this.Result.Summary);
            }
            catch (Exception ex)
            {
                Logger.Error(JsonConvert.SerializeObject(ex.Message));

                this.ScheduleHistoryItem.Succeeded = false;

                this.Errored(ref ex);

                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private LogCompressionResult CompressPreviousMonthLogs()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string logsDirectory = Path.GetFullPath(Path.Combine(baseDirectory, UpendoPromptDirectory));

            DateTime targetMonth = DateTime.Now.AddMonths(-1);
            DateTime targetMonthStart = new DateTime(targetMonth.Year, targetMonth.Month, 1);

            var result = new LogCompressionResult
            {
                LogsDirectory = logsDirectory,
                TargetMonth = targetMonthStart,
                FilesCompressed = 0,
                ZipFilePath = string.Empty,
                Summary = string.Empty
            };

            if (!Directory.Exists(logsDirectory))
            {
                result.Summary = string.Format(
                    CultureInfo.InvariantCulture,
                    "Clear DNN logs skipped because the logs directory does not exist. Logs directory: {0}",
                    logsDirectory);

                return result;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(logsDirectory);

            List<FileInfo> logs = directoryInfo
                .GetFiles("*.*", SearchOption.TopDirectoryOnly)
                .Where(file => this.IsCompressibleLogFile(file, targetMonthStart))
                .OrderBy(file => file.Name)
                .ToList();

            if (logs.Count == 0)
            {
                result.Summary = string.Format(
                    CultureInfo.InvariantCulture,
                    "Clear DNN logs completed with no matching files. Logs directory: {0}. Target month: {1:yyyy-MM}.",
                    logsDirectory,
                    targetMonthStart);

                return result;
            }

            string archiveDirectory = Path.Combine(
                logsDirectory,
                string.Format(CultureInfo.InvariantCulture, "Logs-{0:yyyyMM}", targetMonthStart));

            string zipFilePath = string.Concat(archiveDirectory, ".zip");

            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            if (Directory.Exists(archiveDirectory))
            {
                Directory.Delete(archiveDirectory, true);
            }

            Directory.CreateDirectory(archiveDirectory);

            foreach (FileInfo log in logs)
            {
                string destinationPath = Path.Combine(archiveDirectory, log.Name);

                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                File.Move(log.FullName, destinationPath);
            }

            ZipFile.CreateFromDirectory(archiveDirectory, zipFilePath, CompressionLevel.Optimal, true);

            Directory.Delete(archiveDirectory, true);

            result.FilesCompressed = logs.Count;
            result.ZipFilePath = zipFilePath;
            result.Summary = string.Format(
                CultureInfo.InvariantCulture,
                "Clear DNN logs completed. Files compressed: {0}. Zip file: {1}. Target month: {2:yyyy-MM}.",
                result.FilesCompressed,
                result.ZipFilePath,
                result.TargetMonth);

            return result;
        }

        private bool IsCompressibleLogFile(FileInfo file, DateTime targetMonth)
        {
            if (file == null)
            {
                return false;
            }

            if (file.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            bool isKnownLogFileName = this.TryGetLogDateFromFileName(file.Name, out DateTime fileNameDate);

            if (!isKnownLogFileName)
            {
                return false;
            }

            return this.IsSameYearAndMonth(file.CreationTime, targetMonth)
                || this.IsSameYearAndMonth(file.LastWriteTime, targetMonth)
                || this.IsSameYearAndMonth(fileNameDate, targetMonth);
        }

        private bool TryGetLogDateFromFileName(string fileName, out DateTime logDate)
        {
            logDate = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            Match standardLogMatch = StandardLogFileRegex.Match(fileName);

            if (standardLogMatch.Success)
            {
                return DateTime.TryParseExact(
                    standardLogMatch.Groups["date"].Value,
                    StandardLogDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out logDate);
            }

            Match installerLogMatch = InstallerLogFileRegex.Match(fileName);

            if (installerLogMatch.Success)
            {
                return DateTime.TryParseExact(
                    installerLogMatch.Groups["date"].Value,
                    InstallerLogDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out logDate);
            }

            return false;
        }

        private bool IsSameYearAndMonth(DateTime date, DateTime targetMonth)
        {
            return date.Year == targetMonth.Year && date.Month == targetMonth.Month;
        }
    }
}