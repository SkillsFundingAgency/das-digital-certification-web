using System.IO;
using Aspose.Pdf;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class AsposeLicenseWrapper : IAsposeLicenseWrapper
    {
        public void SetLicense(Stream stream)
        {
            // This will set the license for Aspose.Pdf to avoid evaluation limitations. The license file is expected to be provided as a stream.
            var license = new License();
            license.SetLicense(stream);
        }
    }
}