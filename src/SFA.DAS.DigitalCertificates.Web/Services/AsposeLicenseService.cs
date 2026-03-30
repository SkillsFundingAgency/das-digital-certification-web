using Aspose.Pdf;
using Microsoft.Extensions.Logging;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class AsposeLicenseService : IAsposeLicenseService
    {
        private readonly ILogger<AsposeLicenseService> _logger;
        private readonly DigitalCertificatesWebConfiguration _digitalCertificatesWebConfiguration;
        private readonly IBlobService _blob;

        public AsposeLicenseService(IBlobService blob, DigitalCertificatesWebConfiguration digitalCertificatesWebConfiguration, ILogger<AsposeLicenseService> logger)
        {
            _logger = logger;
            _blob = blob;
            _digitalCertificatesWebConfiguration = digitalCertificatesWebConfiguration;
        }

        public async Task GetAsposeLicense()
        {
            try
            {
                await using var licenseStream = await _blob.OpenBlobReadAsync(_digitalCertificatesWebConfiguration.LicenseBlobName);
                var license = new License();
                license.SetLicense(licenseStream);
                _logger.LogInformation("Aspose license loaded successfully.");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
