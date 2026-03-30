using Azure.Storage.Blobs;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobContainerClient _blobContainerClient;

        public BlobService(DigitalCertificatesWebConfiguration digitalCertificatesWebConfiguration)
        {
            if (string.IsNullOrWhiteSpace(digitalCertificatesWebConfiguration.BlobStorageConnectionString))
                throw new ArgumentException("AzureBlob:ConnectionString is missing.");
            if (string.IsNullOrWhiteSpace(digitalCertificatesWebConfiguration.ContainerName))
                throw new ArgumentException("AzureBlob:ContainerName is missing.");

            var clientOptions = new BlobClientOptions();


            _blobContainerClient = new BlobContainerClient(digitalCertificatesWebConfiguration.BlobStorageConnectionString,
                digitalCertificatesWebConfiguration.ContainerName, clientOptions);
        }

        public async Task<byte[]> GetBlobBytesAsync(string blobName)
        {
            await using var stream = await OpenBlobReadAsync(blobName);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }

        public async Task<Stream> OpenBlobReadAsync(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("blobName is required.", nameof(blobName));

            var blob = _blobContainerClient.GetBlobClient(blobName);

            if (!await blob.ExistsAsync())
                throw new FileNotFoundException($"Blob not found: {blobName}");

            var resp = await blob.DownloadStreamingAsync();
            return resp.Value.Content;
        }
    }
}
