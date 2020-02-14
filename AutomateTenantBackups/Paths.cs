using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutomateTenantBackups
{
    class Paths
    {
        #region User Variables
        public static readonly string LogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "AWSBackupRootFolder","Configuration","LogFile.json");
        //Users document - C:\Users\RAJAU\Documents\AWSBackupRootFolder
        public static readonly string AWSBackupRootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "AWSBackupRootFolder");
        //Users document - C:\Users\RAJAU\Documents\AWSBackupRootFolder\Configuration
        public static readonly string ConfigurationFolder = Path.Combine(AWSBackupRootFolder, "Configuration");
        //Users document - C:\Users\RAJAU\Documents\AWSBackupRootFolder\Configuration\config.json
        public static string configPath = Path.Combine(AWSBackupRootFolder,"Configuration" ,"config.json");
        //Users document - C:\Users\RAJAU\Documents\AWSBackupRootFolder\Configuration\S3Downloader.bat
        public static readonly string usersS3DownloaderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), AWSBackupRootFolder,"Configuration", "S3Downloader.bat");
        public static readonly string AWSArchivesFile = "AWSArchives";
        //Users document - C:\Users\RAJAU\Documents\AWSBackupRootFolder\Configuration\logo.png
        public static readonly string usersAppLogoPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), AWSBackupRootFolder,"Configuration" ,"logo.png");
        //Users document - C:\Users\RAJAU\Documents\AWSBackupRootFolder\AWSArchivesFile.zip
        public static readonly string archiveToUpload = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), AWSBackupRootFolder, $"{AWSArchivesFile}.zip");
        //Users document - C:\Users\RAJAU\Documents\AWSBackupRootFolder\AWSArchives
        public static readonly string AWSArchivesFolder = Path.Combine(AWSBackupRootFolder, AWSArchivesFile);
        #endregion
        #region Program Variables
        public static readonly string appS3DownloaderPath = Path.Combine(Environment.CurrentDirectory, "Files", "S3Downloader.bat");
        public static readonly string appLogoPath = Path.Combine(Environment.CurrentDirectory, "Files", "logo.png");
        #endregion
    }
}
