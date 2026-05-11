using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface IDownloadCertificateService
    {
        DownloadCertificateViewModel CreateDownloadCertificateViewModel(
             DownloadCertificateRequestViewModel request);
    }
}
