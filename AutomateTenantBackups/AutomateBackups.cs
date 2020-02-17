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
        private static bool doesDataExist = false;
        private static ConfigHelper configHelper;
        private static DeleteVault deleteVault;
        #endregion

        static async Task Main(string[] args)
        {
            try
            {
                deleteVault = new DeleteVault();
                configHelper = new ConfigHelper();
                
                CheckIfFoldersExist();

                await Task.Run(() => DownloadS3Bucket());

                IsBackupNeeded();

                await Task.Run(() => ZipFiles());
                await Task.Run(() => StartBackup());
                
                CleanUpFiles();

                await Task.Run(() => deleteVault.DeleteOldestArchiveAsync());
                Notifications.Notify();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\n\nBackup failed !\n\n{ex.ToString()}");
            }
        }

        private static void CopyFilesToUsersDirectory()
        {
            if (!File.Exists(Paths.usersAppLogoPath))
                File.Copy(Paths.appLogoPath, Paths.usersAppLogoPath, true);
            if (!File.Exists(Paths.usersS3DownloaderPath))
                File.Copy(Paths.appS3DownloaderPath, Paths.usersS3DownloaderPath, true);
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
            int fCount = Directory.GetFiles(Paths.AWSArchivesFolder, "*", SearchOption.AllDirectories).Length;
            return fCount;
        }

        private static void CleanUpFiles()
        {
            if (File.Exists(Paths.archiveToUpload))
                File.Delete(Paths.archiveToUpload);

            if (doesDataExist)
                Directory.Delete(Paths.AWSArchivesFolder, recursive: true);
        }

        private static void CheckIfFoldersExist()
        {
            CleanUpFiles();
            CopyFilesToUsersDirectory();

            if (!Directory.Exists(Paths.AWSBackupRootFolder))
                Directory.CreateDirectory(Paths.AWSBackupRootFolder);
            
            if (!Directory.Exists(Paths.AWSArchivesFolder))
                Directory.CreateDirectory(Paths.AWSArchivesFolder);
        }

        //Start the backup transfer to glacier // this method uses the high level api, can be used for small file sizes
        private static async Task<UploadResult> StartBackup()
        {
            try
            {
                Console.WriteLine("\n\nBackup to vault starting... ");
                ArchiveTransferManager manager = new ArchiveTransferManager(ConfigHelper.ReturnEndpoint(configHelper.AWSRegion));
                var archiveId =  await manager.UploadAsync(configHelper.AWSVaultName, "TenantBackup", Paths.archiveToUpload);
                Console.WriteLine("Archive ID: {0}", archiveId.ArchiveId);
                Console.WriteLine($"checksum {archiveId.Checksum}");
                Notifications.WriteLogFile(archiveId.ArchiveId, archiveId.Checksum);
                Console.WriteLine("\n\nTransfer to S3 Glacier vault is complete.");
                System.Threading.Thread.Sleep(5000);
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
                Console.WriteLine("\nStarted S3 Download process, this may take a while...\n\n ");
                await Task.Run(() =>
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = Paths.usersS3DownloaderPath;
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
                    ZipFile.CreateFromDirectory(Paths.AWSArchivesFolder, $"{Paths.AWSArchivesFolder}.zip");
                }
            });
        }
    }
}
