using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using System;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class DownloadCertificateService : IDownloadCertificateService
    {
        public DownloadCertificateViewModel CreateDownloadCertificateViewModel(
            DownloadCertificateRequestViewModel request)
        {
            ValidateRequiredData(request);

            return new DownloadCertificateViewModel
            {
                FamilyName = request.FamilyName!,
                GivenNames = request.GivenNames!,
                CourseName = request.CourseName!,
                CourseOption = request.CourseOption,
                CourseLevel = request.CourseLevel!,
                OverallGrade = request.OverallGrade,
                DateAwarded = request.DateAwarded!.Value,                
                CertificateNumber = request.CertificateNumber,
                CoronationEmblem = request.CoronationEmblem,
                CertificateType = request.CertificateType
            };
        }

        private static void ValidateRequiredData(DownloadCertificateRequestViewModel request)
        {
            if (string.IsNullOrWhiteSpace(request.FamilyName)
                || string.IsNullOrWhiteSpace(request.GivenNames)
                || string.IsNullOrWhiteSpace(request.CourseName)
                || string.IsNullOrWhiteSpace(request.CourseLevel)
                || request.DateAwarded == null)
            {
                ThrowMissingDataException(request.CertificateId);
            }

            switch (request.CertificateType)
            {
                case CertificateType.Standard: 
                    if (string.IsNullOrWhiteSpace(request.OverallGrade)
                        || string.IsNullOrWhiteSpace(request.CertificateNumber))
                    {
                        ThrowMissingDataException(request.CertificateId);
                    }

                    break;

                case CertificateType.Framework:
                    if (string.IsNullOrWhiteSpace(request.CertificateNumber))
                    {
                        ThrowMissingDataException(request.CertificateId);
                    }

                    break;

                default:
                    throw new InvalidOperationException(
                        $"Certificate {request.CertificateId} has unsupported certificate type.");
            }
        }
      
        private static void ThrowMissingDataException(Guid certificateId)
        {
            throw new InvalidOperationException(
                $"Certificate {certificateId} is missing required data.");
        }
    }       
}

