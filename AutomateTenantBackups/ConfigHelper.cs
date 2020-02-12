using Newtonsoft.Json;
using System;
using System.IO;
using Amazon;

namespace AutomateTenantBackups
{
    class Config
    {
        public Config(string Region, string vaultName)
        {
            AWSRegion = Region;
            AWSVaultName = vaultName;
        }

        public string AWSRegion;
        public string AWSVaultName;
    }

    class ConfigHelper
    {
        public string AWSVaultName { get; set; }
        public string AWSRegion { get; set; }

        private string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), AutomateBackups.aBiPBackupRootFolder, "config.json");
        public ConfigHelper()
        {
            if (!File.Exists(configPath))
            {
                Config config = new Config
                (
                    "eu-west-2",
                    "VaultName"
                );
                
                string result = JsonConvert.SerializeObject(config);

                using (var tw = new StreamWriter(configPath, true))
                {
                    tw.WriteLine(result.ToString());
                    tw.Close();
                }
            }
            else
            {
                ReadConfigFile();
            }
        }

        public static RegionEndpoint ReturnEndpoint(string endpoint)
        {
           return  RegionEndpoint.GetBySystemName(endpoint);
        }

        public void ReadConfigFile()
        {
            using (StreamReader r = new StreamReader(configPath))
            {
                string json = r.ReadToEnd();
                var config = JsonConvert.DeserializeObject<Config>(json);
                AWSRegion = config.AWSRegion;
                AWSVaultName = config.AWSVaultName;
                Console.WriteLine($"You will be backing data to vault: {AWSVaultName} with region: {AWSRegion}");
            }
        }
    }
}
