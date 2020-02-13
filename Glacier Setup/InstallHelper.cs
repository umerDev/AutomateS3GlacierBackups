using Newtonsoft.Json;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Glacier_Setup
{
    class InstallHelper
    {
        private static readonly string AWSBackupRootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "AWSBackupRootFolder");
        private static readonly string configurationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), AWSBackupRootFolder, "Configuration");
        private static readonly string usersS3DownloaderPath = Path.Combine(configurationFolder, "S3Downloader.bat");
        private static string configPath = Path.Combine(configurationFolder, "config.json");

        private string InstallToast = "Install-Module -Name BurntToast";
        private string InstallAWSTools = "Install-Module -Name AWS.Tools.Installer -Force";
        private string InstallScope = "-Scope CurrentUser";

        public string AWSVaultName;
        public string AWSRegion;
        public List<string> BucketNames = new List<string>();

        public InstallHelper()
        {
            Directory.CreateDirectory(AWSBackupRootFolder);
            Directory.CreateDirectory(configurationFolder);
        }

        public void BucketsToBackup()
        {
            bool showMenu = true;
            while (showMenu)
            {
                showMenu = MainMenu();
            }
            WriteBatchDownloader();
        }

        private void WriteBatchDownloader()
        {
            using (StreamWriter file = new StreamWriter(usersS3DownloaderPath))
            {
                file.WriteLine("@echo off");
                file.WriteLine("title AWS Backup");
                file.WriteLine("echo Welcome to AWS Backup");
                file.WriteLine("color a");
                file.WriteLine();
                file.Close();
            }

            foreach (var i in BucketNames)
            {
                using (StreamWriter sw = File.AppendText(usersS3DownloaderPath))
                {
                    sw.WriteLine($@"echo You are going to copy bucket {i}");
                    sw.WriteLine($@"aws s3 sync s3://{i} %UserProfile%\Documents\AWSBackupRootFolder\AWSArchives\Today");
                    sw.WriteLine("echo Finished download");
                    sw.WriteLine("setlocal enableextensions");
                    sw.WriteLine(@"set todaysDate=%DATE:/=_%");
                    sw.WriteLine($@"Rename %UserProfile%\Documents\AWSBackupRootFolder\AWSArchives\Today {i}_""%todaysDate%"" ");
                    sw.WriteLine("echo Renamed Folder");
                    sw.WriteLine();
                }
            }
        }

        private void BucketInput()
        {
            Console.WriteLine("\nEnter Bucket Name");
            var input = Console.ReadLine();
            if (String.IsNullOrEmpty(input))
                return;
            BucketNames.Add(input);
            Console.WriteLine("Bucket added");
            Console.WriteLine("\n\nPress enter to continue");
            Console.ReadKey();
        }

        private bool MainMenu()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("\r\n Select an option: ");   
                Console.WriteLine("1) Add new bucket...");
                Console.WriteLine("2) Save \n");

                switch (Console.ReadLine())
                {
                    case "1":
                        BucketInput();
                        return true;
                    case "2":
                        return false;
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        
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
