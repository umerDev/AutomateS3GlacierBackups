using Newtonsoft.Json;
using System;
using System.IO;

namespace Glacier_Setup
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

    class GlacierSetup
    {
        private static InstallHelper installHelper;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to AWS S3 Glacier setup");
            bool showMenu = true;
            while (showMenu)
            {
                showMenu = MainMenu();
            }

        }
        private static bool MainMenu()
        {
            try
            {
                installHelper = new InstallHelper();
                Console.Clear();
                Console.WriteLine("\nWelcome to AWS S3 Glacier backup setup\n");
                Console.WriteLine("\n\nChoose an option:\n\n");
                Console.WriteLine("1) Install AWS CLI");
                Console.WriteLine("2) Setup AWS Config for Vault");
                Console.WriteLine("3) Set AWS credentials");
                Console.WriteLine("4) Install BurntToast");
                Console.WriteLine("5) Enter Buckets to Download");
                Console.WriteLine("6) Exit");
                Console.Write("\r\nSelect an option: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        installHelper.InstallAWSCLI();
                        return true;
                    case "2":
                        installHelper.SetupAWSConfig();
                        return true;
                    case "3":
                        installHelper.SetAWSCredentials();
                        return true;
                    case "4":
                        installHelper.InstallBurntToast();
                        return true;
                    case "5":
                        installHelper.BucketsToBackup();
                        return true;
                    case "6":
                        return false;
                    default:
                        return true;
                }
            }
            catch (Exception e)
            { 
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
