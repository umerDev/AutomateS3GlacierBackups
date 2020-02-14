using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace AutomateTenantBackups
{
    [Serializable]
    class LogRoot
    {
        public List<LogFile> LogList = new List<LogFile>();
    }
    [Serializable]
    class LogFile
    {
        public LogFile(string ArchiveId, string CheckSum, string LogNumber, DateTime DateTime)
        {
            logNumber = LogNumber;
            archiveID = ArchiveId;
            checkSum = CheckSum;
            dateTime = DateTime;
        }
        public string logNumber;
        public string archiveID;
        public string checkSum;
        public DateTime dateTime;
    }

    /// <summary>
    /// This class displays a notification in the windows tray to update the user regarding the completion of the backup.
    /// </summary>
    class Notifications
    {
        private static readonly string NotifyShellScript = Path.Combine(Environment.CurrentDirectory, "Files", "Notifications.ps1");

        public static void Notify()
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = @"C:\windows\system32\windowspowershell\v1.0\powershell.exe";
            process.StartInfo.Arguments = "\"&'" + NotifyShellScript + "'\"";
            process.Start();
            process.WaitForExit();
        }

        public static void WriteLogFile(string archiveID, string checkSum)
        {
            LogFile logFile = new LogFile(archiveID, checkSum, Guid.NewGuid().ToString(), DateTime.Now);
            LogRoot logRoot = new LogRoot();
            logRoot.LogList.Add(logFile);

            if (AppendLogFile(logFile))
                return;

            var JsonLogFile = JsonConvert.SerializeObject(logRoot);
            using (StreamWriter file = new StreamWriter(Paths.LogFile, true))
            {
                file.WriteLine(JsonLogFile);
            }
        }

        public static bool AppendLogFile(LogFile logFile)
        {
            if (File.Exists(Paths.LogFile))
            {
                var jsonData = File.ReadAllText(Paths.LogFile);
                // De-serialize to object or create new list
                LogRoot  logR = new LogRoot();
                LogRoot logRawText = JsonConvert.DeserializeObject<LogRoot>(jsonData);

                // Add any new log data
                logRawText.LogList.Add(logFile);

                // Update json data string
                jsonData = JsonConvert.SerializeObject(logRawText);
                System.IO.File.WriteAllText(Paths.LogFile, jsonData);
                return true;
            }
            return false;
        }
    }
}


