using Amazon.Glacier;
using Amazon.Glacier.Model;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AutomateTenantBackups
{
    class AWSMultipartUploader
    {
        private Amazon.RegionEndpoint Endpoint;
        private string VaultName = "";
        private string ArchiveToUpload = "";
        private long partSize = 4194304; // 4 MB.
        public AWSMultipartUploader(string vaultName,string archive,Amazon.RegionEndpoint endpoint)
        {
            Endpoint = endpoint;
            VaultName = vaultName;
            ArchiveToUpload = archive;
        }

        public async Task StartMultipartUploadAsync()
        {
            AmazonGlacierClient client;
            List<string> partChecksumList = new List<string>();
            try
            {
                using (client = new AmazonGlacierClient(Endpoint))
                {
                    Console.WriteLine("Uploading an archive.");
                    string uploadId = await InitiateMultipartUploadAsync(client);
                    partChecksumList = UploadParts(uploadId, client);
                    string archiveId = await CompleteMPUAsync(uploadId, client, partChecksumList);
                    Console.WriteLine("Total parts: {0}", partChecksumList.Count);
                    Console.WriteLine("Upload ID: {0}", uploadId);
                    Console.WriteLine("Archive ID: {0}", archiveId);
                }
                Console.WriteLine("Operations successful.");
            }
            catch (AmazonGlacierException e) { Console.WriteLine(e.Message); }
            catch (AmazonServiceException e) { Console.WriteLine(e.Message); }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        private async Task<string> InitiateMultipartUploadAsync(AmazonGlacierClient client)
        {
            InitiateMultipartUploadRequest initiateMPUrequest = new InitiateMultipartUploadRequest()
            {
                VaultName = VaultName,
                PartSize = partSize,
                ArchiveDescription = $"aBiPBackup_{DateTime.Now}."
            };

            InitiateMultipartUploadResponse initiateMPUresponse = await client.InitiateMultipartUploadAsync(initiateMPUrequest);

            return initiateMPUresponse.UploadId;
        }

        private List<string> UploadParts(string uploadID, AmazonGlacierClient client)
        {
            List<string> partChecksumList = new List<string>();
            long currentPosition = 0;
            var buffer = new byte[Convert.ToInt32(partSize)];

            long fileLength = new FileInfo(ArchiveToUpload).Length;
            Console.WriteLine($"Total file size {fileLength}");
            using (FileStream fileToUpload = new FileStream(ArchiveToUpload, FileMode.Open, FileAccess.Read))
            {
                while (fileToUpload.Position < fileLength)
                {
                    Stream uploadPartStream = GlacierUtils.CreatePartStream(fileToUpload, partSize);
                    string checksum = TreeHashGenerator.CalculateTreeHash(uploadPartStream);
                    partChecksumList.Add(checksum);
                    // Upload part.
                    UploadMultipartPartRequest uploadMPUrequest = new UploadMultipartPartRequest()
                    {

                        VaultName = VaultName,
                        Body = uploadPartStream,
                        Checksum = checksum,
                        UploadId = uploadID
                    };
                    uploadMPUrequest.SetRange(currentPosition, currentPosition + uploadPartStream.Length - 1);
                    client.UploadMultipartPartAsync(uploadMPUrequest);

                    currentPosition = currentPosition + uploadPartStream.Length;
                }
            }
            return partChecksumList;
        }

        private async Task<string> CompleteMPUAsync(string uploadID, AmazonGlacierClient client, List<string> partChecksumList)
        {
            long fileLength = new FileInfo(ArchiveToUpload).Length;
            CompleteMultipartUploadRequest completeMPUrequest = new CompleteMultipartUploadRequest()
            {
                UploadId = uploadID,
                ArchiveSize = fileLength.ToString(),
                Checksum = TreeHashGenerator.CalculateTreeHash(partChecksumList),
                VaultName = VaultName
            };

            CompleteMultipartUploadResponse completeMPUresponse = await client.CompleteMultipartUploadAsync(completeMPUrequest);
            return completeMPUresponse.ArchiveId;
        }
    }
}
