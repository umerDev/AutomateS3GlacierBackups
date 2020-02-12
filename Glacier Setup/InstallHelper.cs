using Newtonsoft.Json;
using System;
using System.IO;
using System.Diagnostics;

namespace Glacier_Setup
{
    class InstallHelper
    {
        private static readonly string AWSBackupRootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "AWSBackupRootFolder");
        private static string configPath = Path.Combine(AWSBackupRootFolder, "config.json");
       
        private string InstallToast = "Install-Module -Name BurntToast";
        private string InstallAWSTools = "Install-Module -Name AWS.Tools.Installer -Force";
        private string InstallScope = "-Scope CurrentUser";

        public string AWSVaultName;
        public string AWSRegion;

        public void InstallAWSCLI()
        {
            Process CMDprocess = new System.Diagnostics.Process();
            var StartProcessInfo = new System.Diagnostics.ProcessStartInfo();
            StartProcessInfo.FileName = @"C:\Windows\SysWOW64\WindowsPowershell\v1.0\powershell.exe";
            StartProcessInfo.Verb = "runas";
            StartProcessInfo.Arguments = $"{InstallAWSTools} {InstallScope}";
            CMDprocess.StartInfo = StartProcessInfo;
            CMDprocess.Start();
            CMDprocess.WaitForExit();
            Console.WriteLine("Installed AWS Cli tools, Press enter to continue.");
            Console.Read();
        }

        public void SetAWSCredentials()
        {
            Process CMDprocess = new Process();
            var StartProcessInfo = new ProcessStartInfo();
            StartProcessInfo.FileName = @"C:\Windows\SysWOW64\WindowsPowershell\v1.0\powershell.exe";
            StartProcessInfo.Verb = "runas";
            StartProcessInfo.Arguments = "aws configure";
            CMDprocess.StartInfo = StartProcessInfo;
            CMDprocess.Start();
            CMDprocess.WaitForExit();
            Console.WriteLine("Successfully set AWS credentials, Press enter to continue.");
            Console.Read();
        }

        public void InstallBurntToast()
        {
            Process CMDprocess = new System.Diagnostics.Process();
            var StartProcessInfo = new System.Diagnostics.ProcessStartInfo();
            StartProcessInfo.FileName = @"C:\Windows\SysWOW64\WindowsPowershell\v1.0\powershell.exe";
            StartProcessInfo.Verb = "runas";
            StartProcessInfo.Arguments = $"{InstallToast} {InstallScope}";

            CMDprocess.StartInfo = StartProcessInfo;
            CMDprocess.Start();
            CMDprocess.WaitForExit();
            Console.WriteLine("Installed Burnt Toast, Press enter to continue.");
            Console.Read();
        }

        public void SetupAWSConfig()
        {
            EnterAWSRegion();
            EnterAWSVaultName();
            if (string.IsNullOrEmpty(AWSRegion))
            {
                EnterAWSRegion();
                if (string.IsNullOrEmpty(AWSVaultName))
                {
                    EnterAWSVaultName();
                }
            }
            Directory.CreateDirectory(AWSBackupRootFolder);
            if (File.Exists(configPath))
                File.Delete(configPath);

            Config config = new Config
            (
                AWSRegion,
                AWSVaultName
            );

            string result = JsonConvert.SerializeObject(config);

            using (var tw = new StreamWriter(configPath, true))
            {
                tw.WriteLine(result.ToString());
                tw.Close();
            }
            Console.WriteLine("Successfully set AWS config for vault, Press enter to continue.");
            Console.Read();
        }

        private void EnterAWSRegion()
        {
            Console.WriteLine("Enter AWS Region");
            AWSRegion = Console.ReadLine();
        }

        private void EnterAWSVaultName()
        {
            Console.WriteLine("Enter AWS vault name");
            AWSVaultName = Console.ReadLine();
        }
    }
}
