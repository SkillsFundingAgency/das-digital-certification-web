using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly DigitalCertificatesWebConfiguration _config;
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient, IOptions<DigitalCertificatesWebConfiguration> options, ILogger<BlobService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _config = options.Value;
            _logger = logger;
        }

        public async Task<byte[]> GetBlobBytesAsync(string containerName, string blobName)
        {
            await using var stream = await OpenBlobReadAsync(containerName, blobName);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }

        public async Task<Stream> OpenBlobReadAsync(string containerName, string blobName)
        {           
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("Container Name is required.", nameof(containerName));

            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("Blob Name is required.", nameof(blobName));

            try
            {
                var clientOptions = new BlobClientOptions();
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(blobName);

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
