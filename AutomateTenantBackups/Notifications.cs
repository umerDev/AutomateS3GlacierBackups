using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace AutomateTenantBackups
{
    class LogRoot
    {
        public LogRoot(string LogNumber,LogFile LogFile)
        {
            logNumber = LogNumber;
            logFile = LogFile;
        }
        public string logNumber;
        public LogFile logFile;
    }
    class LogFile
    {
        public LogFile(string ArchiveId, string CheckSum, DateTime DateTime)
        {
            archiveID = ArchiveId;
            checkSum = CheckSum;
            dateTime = DateTime;
        }
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

        public static void WriteLogFile(string archiveID, string checksum)
        {
            LogFile logFile = new LogFile(archiveID, checksum, DateTime.Now);
            LogRoot logRoot = new LogRoot(Guid.NewGuid().ToString(), logFile);
            
            var JsonLogFile = JsonConvert.SerializeObject(logRoot);
            using (StreamWriter file = new StreamWriter(Paths.LogFile, true))
            {
                file.WriteLine(JsonLogFile);
            }
        }
    }
}


