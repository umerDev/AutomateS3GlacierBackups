using Amazon.Glacier;
using Amazon.Glacier.Transfer;
using Amazon.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

/// <summary>
/// Currently, there is no method of directly copying from s3 to a specific s3 glacier vault
/// for reference: https://forums.aws.amazon.com/thread.jspa?messageID=695231
/// 
/// This program helps to automate backups from aws s3 buckets to aws s3 glacier
/// The flow for the program is: download entire bucket to local machine -> archive to zip -> upload Zip to specified vault
/// 
/// dotnet publish -c Debug -r win10-x64
/// </summary>

namespace AutomateTenantBackups
{
    class AutomateBackups
    {
        #region variables
        public static readonly string AWSBackupRootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Documents", "AWSBackupRootFolder");
        private static readonly string AWSArchivesFile = "AWSArchives";
        private static readonly string AWSArchivesFolder = Path.Combine(AWSBackupRootFolder, AWSArchivesFile);
        private static readonly string awsCLITool = Path.Combine(Environment.CurrentDirectory, "Files", "S3Downloader.bat");
        private static readonly string archiveToUpload = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), AWSBackupRootFolder, $"{AWSArchivesFile}.zip");
        private static readonly string appLogoPath = Path.Combine(Environment.CurrentDirectory, "Files", "logo.png");
        public static readonly string usersAppLogoPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), AWSBackupRootFolder, "logo.png");
        private static bool doesDataExist = false;
        private static ConfigHelper configHelper;
        #endregion

        static async Task Main(string[] args)
        {
            try
            {
                configHelper = new ConfigHelper();
                CheckIfFoldersExist();

                await Task.Run(() => DownloadS3Bucket());

                IsBackupNeeded();

                await Task.Run(() => ZipFiles());

                AWSMultipartUploader multipartUploader = new AWSMultipartUploader(configHelper.AWSVaultName, archiveToUpload, ConfigHelper.ReturnEndpoint(configHelper.AWSRegion));
                await multipartUploader.StartMultipartUploadAsync();
                
                CleanUpFiles();
                Notifications.Notify();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\n\nBackup failed !\n\n{ex.ToString()}");
            }
        }

        private static void CopyAppLogo()
        {
            if (!File.Exists(usersAppLogoPath))
                File.Copy(appLogoPath, usersAppLogoPath, true);
        }
        private static void IsBackupNeeded()
        {
            if (!doesDataExist)
            {
                Console.WriteLine("\n\nNo data to backup\n\n");
                System.Threading.Thread.Sleep(5000);
                Environment.Exit(0);
            }
        }

        private static int CheckFilesInDirectory()
        {
            int fCount = Directory.GetFiles(AWSArchivesFolder, "*", SearchOption.AllDirectories).Length;
            return fCount;
        }

        private static void CleanUpFiles()
        {
            if (File.Exists(archiveToUpload))
                File.Delete(archiveToUpload);

            if (doesDataExist)
                Directory.Delete(AWSArchivesFolder, recursive: true);
        }

        private static void CheckIfFoldersExist()
        {
            CleanUpFiles();
            CopyAppLogo();

            if (!Directory.Exists(AWSBackupRootFolder))
                Directory.CreateDirectory(AWSBackupRootFolder);
            
            if (!Directory.Exists(AWSArchivesFolder))
                Directory.CreateDirectory(AWSArchivesFolder);
        }

        //Start the backup transfer to glacier // this method uses the high level api, can be used for small file sizes
        private static async Task<UploadResult> StartBackup()
        {
            try
            {
                Console.WriteLine("\n\nBackup to vault starting... ");
                ArchiveTransferManager manager = new ArchiveTransferManager(ConfigHelper.ReturnEndpoint(configHelper.AWSRegion));
                var archiveId =  await manager.UploadAsync(configHelper.AWSVaultName, "TenantBackup", archiveToUpload);
                Console.WriteLine("Archive ID: {0}", archiveId.ArchiveId);
                Console.WriteLine($"checksum {archiveId.Checksum}");
                Console.WriteLine("To continue, press Enter");
                Console.ReadKey();
                Console.WriteLine("\n\nTransfer to S3 Glacier vault is complete.");
                return archiveId;
            }
            catch (AmazonGlacierException e) { Console.WriteLine(e.Message); }
            catch (AmazonServiceException e) { Console.WriteLine(e.Message); }
            catch (Exception e) { Console.WriteLine(e.Message); }
            Console.WriteLine("To continue, press Enter");
            Console.ReadKey();
            return null;
        }

        //launch batch file to download buckets
        private static async Task DownloadS3Bucket()
        {
            try
            {
                Console.WriteLine("\nStarting S3 Download process...\n\n ");
                await Task.Run(() =>
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = awsCLITool;
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    proc.Start();
                    proc.WaitForExit();
                    int exitCode = proc.ExitCode;
                    proc.Close();

                    if (CheckFilesInDirectory() > 1)
                        doesDataExist = true;
                    else
                        doesDataExist = false;
                });
            }
            catch (Exception e) { doesDataExist = false; }
        }

        private static async Task ZipFiles()
        {
            await Task.Run(() =>
            {
                if (doesDataExist)
                {
                    Console.WriteLine("\n\nCompressing files...");
                    ZipFile.CreateFromDirectory(AWSArchivesFolder, $"{AWSArchivesFile}.zip");
                }
            });
        }
    }
}
