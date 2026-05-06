using System;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharedFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharedStandardCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;

namespace SFA.DAS.DigitalCertificates.Web.Extensions
{
    public static class GetCertificateQueryResultExtension
    {
        public static DownloadCertificateRequestViewModel ToDownloadCertificateRequest(
            this GetStandardCertificateQueryResult result,
            Guid certificateId)
        {        
            return new DownloadCertificateRequestViewModel
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                FamilyName = result.FamilyName,
                GivenNames = result.GivenNames,
                CourseName = result.CourseName,
                CourseOption = result.CourseOption,
                CourseLevel = result.CourseLevel?.ToString(),
                DateAwarded = result.DateAwarded,
                OverallGrade = result.OverallGrade,
                CertificateNumber = result.CertificateReference,
                CoronationEmblem = result.CoronationEmblem
            };
        }

        public static DownloadCertificateRequestViewModel ToDownloadCertificateRequest(
            this GetSharedStandardCertificateQueryResult result,
            Guid certificateId)
        {
            return new DownloadCertificateRequestViewModel
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                FamilyName = result.FamilyName,
                GivenNames = result.GivenNames,
                CourseName = result.CourseName,
                CourseOption = result.CourseOption,
                CourseLevel = result.CourseLevel?.ToString(),
                DateAwarded = result.DateAwarded,
                OverallGrade = result.OverallGrade,
                CertificateNumber = result.CertificateReference,
                CoronationEmblem = result.CoronationEmblem
            };
        }

        public static DownloadCertificateRequestViewModel ToDownloadCertificateRequest(
            this GetFrameworkCertificateQueryResult result,
            Guid certificateId)
        {
            return new DownloadCertificateRequestViewModel
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Framework,
                FamilyName = result.FamilyName,
                GivenNames = result.GivenNames,
                CourseName = result.CourseName,
                CourseOption = result.CourseOption,
                CourseLevel = result.CourseLevel,
                DateAwarded = result.DateAwarded,
                CertificateNumber = result.FrameworkCertificateNumber
            };
        }

        public static DownloadCertificateRequestViewModel ToDownloadCertificateRequest(
            this GetSharedFrameworkCertificateQueryResult result,
            Guid certificateId)
        {
            return new DownloadCertificateRequestViewModel
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Framework,
                FamilyName = result.FamilyName,
                GivenNames = result.GivenNames,
                CourseName = result.CourseName,
                CourseOption = result.CourseOption,
                CourseLevel = result.CourseLevel,
                DateAwarded = result.DateAwarded,
                CertificateNumber = result.FrameworkCertificateNumber
            };
        }        
    }
}