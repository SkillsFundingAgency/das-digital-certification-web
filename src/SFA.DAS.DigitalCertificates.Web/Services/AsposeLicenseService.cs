using Microsoft.Extensions.Logging;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class AsposeLicenseService : IAsposeLicenseService
    {
        private const string LicenseRetrieveErrorMessage = "An error occurred while retrieving the Aspose license.";
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LicenseRetrieveErrorMessage);
                throw new InvalidOperationException(LicenseRetrieveErrorMessage, ex);
            }
        }
    }
}
