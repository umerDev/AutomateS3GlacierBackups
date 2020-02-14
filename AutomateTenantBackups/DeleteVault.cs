using Amazon.Glacier;
using Amazon.Glacier.Transfer;
using Amazon.Runtime;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Amazon;
using System.Threading;

namespace AutomateTenantBackups
{
    class DeleteVault
    {
        private string archiveToDelete;
        private string vaultName;
        private RegionEndpoint region;

        private bool GetOldestArchive()
        {
            //load config file
            var jsonConfigData = File.ReadAllText(Paths.configPath);
            var config = JsonConvert.DeserializeObject<Config>(jsonConfigData);
            region = ConfigHelper.ReturnEndpoint(config.AWSRegion);
            vaultName = config.AWSVaultName;

            //load log file
            var jsonData = File.ReadAllText(Paths.LogFile);
            LogRoot logR = new LogRoot();
            LogRoot logRawText = JsonConvert.DeserializeObject<LogRoot>(jsonData);

            //if there's only one stored archive, skip delete process
            if (logRawText.LogList.Count <= 1)
                return false;

            var archive = logRawText.LogList[0];
            archiveToDelete = archive.archiveID;
            return true;
        }

        //Remove the deleted archive from the config file.
        private void DeleteArchiveFromConfig()
        {
            var jsonData = File.ReadAllText(Paths.LogFile);
            LogRoot logR = new LogRoot();
            LogRoot logRawText = JsonConvert.DeserializeObject<LogRoot>(jsonData);

            // Delete the top record
            logRawText.LogList.RemoveAt(0);

            // Update json data string
            jsonData = JsonConvert.SerializeObject(logRawText);
            File.WriteAllText(Paths.LogFile, jsonData);
        }

        public async Task DeleteOldestArchiveAsync()
        {
            try
            {
                if (GetOldestArchive())
                {
                    var manager = new ArchiveTransferManager(region);
                    await manager.DeleteArchiveAsync(vaultName, archiveToDelete);
                    DeleteArchiveFromConfig();
                }
            }
            catch (AmazonGlacierException e) { Console.WriteLine(e.Message); }
            catch (AmazonServiceException e) { Console.WriteLine(e.Message); }
            catch (Exception e) { Console.WriteLine(e.Message); }
            Console.WriteLine($"You successfully deleted archive {archiveToDelete} from Vault {vaultName}");
            Thread.Sleep(500);
        }
    }
}
