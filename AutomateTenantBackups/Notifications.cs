using System;
using System.Diagnostics;
using System.IO;

namespace AutomateTenantBackups
{

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
            using (StreamWriter file = new StreamWriter(Paths.LogFile, true))
            {
                file.WriteLine($"Archive ID: {archiveID} \nChecksum: {checksum} \n Date:{DateTime.Now}");
            }
        }
    }
}


