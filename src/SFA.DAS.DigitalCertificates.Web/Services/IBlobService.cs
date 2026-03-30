using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface IBlobService
    {
        Task<byte[]> GetBlobBytesAsync(string blobName);
        Task<Stream> OpenBlobReadAsync(string blobName);
    }
}
