using Microsoft.Extensions.Logging;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class AsposeLicenseService : IAsposeLicenseService
    {
        private const string LicenseLoaddedMessage = "Aspose license loaded successfully.";
        private const string LicenseRetrieveError = "error occured retrieving the aspose license";
        private readonly ILogger<AsposeLicenseService> _logger;
        private readonly DigitalCertificatesWebConfiguration _digitalCertificatesWebConfiguration;
        private readonly IBlobService _blob;

        private readonly IAsposeLicenseWrapper _licenseWrapper;

        public AsposeLicenseService(IBlobService blob, DigitalCertificatesWebConfiguration digitalCertificatesWebConfiguration, ILogger<AsposeLicenseService> logger, IAsposeLicenseWrapper licenseWrapper)
        {
            _logger = logger;
            _blob = blob;
            _digitalCertificatesWebConfiguration = digitalCertificatesWebConfiguration;
            _licenseWrapper = licenseWrapper;
        }

        public async Task GetAsposeLicense()
        {
            try
            {
                await using var licenseStream = await _blob.OpenBlobReadAsync(_digitalCertificatesWebConfiguration.AsposeLicenseContainerName, _digitalCertificatesWebConfiguration.LicenseBlobName);
                _licenseWrapper.SetLicense(licenseStream);
                _logger.LogInformation(LicenseLoaddedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LicenseRetrieveError);
            }
        }
    }
}
