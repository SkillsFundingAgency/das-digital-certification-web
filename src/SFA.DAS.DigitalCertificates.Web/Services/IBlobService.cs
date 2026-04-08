using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface IBlobService
    {
        Task<byte[]> GetBlobBytesAsync(string containerName, string blobName);
        Task<Stream> OpenBlobReadAsync(string containerName, string blobName);
    }
}
