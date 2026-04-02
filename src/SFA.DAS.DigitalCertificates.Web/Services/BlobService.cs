using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class BlobService : IBlobService
    {
        private const string Message = "Unable to get blob from azure storage.";
        private readonly ILogger<BlobService> _logger;
        private readonly BlobContainerClient _blobContainerClient;

        public BlobService(DigitalCertificatesWebConfiguration digitalCertificatesWebConfiguration, ILogger<BlobService> logger)
        {
            if (string.IsNullOrWhiteSpace(digitalCertificatesWebConfiguration.BlobStorageConnectionString))
                throw new ArgumentException("DigitalCertificatesWebConfiguration:BlobStorageConnectionString is missing.");
            if (string.IsNullOrWhiteSpace(digitalCertificatesWebConfiguration.ContainerName))
                throw new ArgumentException("DigitalCertificatesWebConfiguration:ContainerName is missing.");

            var clientOptions = new BlobClientOptions();

            _blobContainerClient = new BlobContainerClient(digitalCertificatesWebConfiguration.BlobStorageConnectionString,
                digitalCertificatesWebConfiguration.ContainerName, clientOptions);

            _logger = logger;
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

            try
            {
                var blobClient = _blobContainerClient.GetBlobClient(blobName);
                if (!await blobClient.ExistsAsync())
                    throw new FileNotFoundException($"Blob not found: {blobName}");

                var resp = await blobClient.DownloadStreamingAsync();

                return resp.Value.Content;

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, Message);
                throw;
            }
        }
    }
}
