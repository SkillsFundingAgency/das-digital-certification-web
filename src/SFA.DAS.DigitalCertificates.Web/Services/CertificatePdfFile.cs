namespace SFA.DAS.DigitalCertificates.Web.Services
{   
    public sealed record CertificatePdfFile(byte[] Content, string ContentType, string filename);  

}
