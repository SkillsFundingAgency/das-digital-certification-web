using Aspose.Pdf;
using Aspose.Pdf.Forms;
using MediatR;
using Microsoft.AspNetCore.Http;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class CertificatesOrchestrator : BaseOrchestrator, ICertificatesOrchestrator
    {
        private readonly ISessionService _sessionService;
        private readonly IUserService _userService;
        private readonly IBlobService _blob;
        private readonly IAsposeLicenseService _asposeLicenseService;
        private readonly DigitalCertificatesWebConfiguration _digitalCertificatesWebConfiguration;
        private const string Level = "Level";
        private const string FullName = "Full Name";
        private const string PassedInfo = "Passed info";
        private const string AchievedGrade = "Achieved grade";
        private const string AwardedOn = "Awarded on";
        private const string DateFormat = "d MMMM yyyy";
        private const string CertificateNumber = "Certificate no";

        public CertificatesOrchestrator(IMediator mediator, 
            IHttpContextAccessor httpContextAccessor, 
            ISessionService sessionService, 
            IUserService userService, 
            IBlobService blob,
            IAsposeLicenseService apposeLicenseService,
            DigitalCertificatesWebConfiguration digitalCertificatesPdfConfiguration)
            : base(mediator, httpContextAccessor)
        {
            _sessionService = sessionService;
            _userService = userService;
            _blob = blob;
            _asposeLicenseService = apposeLicenseService;
            _digitalCertificatesWebConfiguration = digitalCertificatesPdfConfiguration;
        }

        public async Task<CertificatesListViewModel> GetCertificatesListViewModel()
        {
            return new CertificatesListViewModel
            {
                Certificates = await _sessionService.GetOwnedCertificatesAsync(_userService.GetGovUkIdentifier())
            };
        }

        public async Task<DownloadCertificateViewModel?> GetDownloadCertificateViewModelAsync(Guid certificateId)
        {
            var result = await Mediator.Send(new GetStandardCertificateQuery { CertificateId = certificateId });

            if (result == null)
                return null;

            if (string.IsNullOrWhiteSpace(result.FamilyName)
                || string.IsNullOrWhiteSpace(result.GivenNames)
                || string.IsNullOrWhiteSpace(result.CourseName)
                || result.CourseLevel == null
                || result.DateAwarded == null
                || string.IsNullOrWhiteSpace(result.OverallGrade)
                || string.IsNullOrWhiteSpace(result.CertificateReference))
            {
                throw new InvalidOperationException($"Certificate {certificateId} is missing required data.");
            }

            var viewModel = new DownloadCertificateViewModel
            {
                FamilyName = result.FamilyName!,
                GivenNames = result.GivenNames!,
                CourseName = result.CourseName!,
                CourseOption = result.CourseOption,
                CourseLevel = result.CourseLevel.Value.ToString(),
                OverallGrade = result.OverallGrade!,
                DateAwarded = result.DateAwarded.Value,
                CertificateNumber = result.CertificateReference!,
                CoronationEmblem = result.CoronationEmblem,
                CertificateType = CertificateType.Standard
            };

            return viewModel;
        }

        public async Task<CertificateStandardViewModel?> GetCertificateStandardViewModel(Guid certificateId)
        {
            var result = await Mediator.Send(new GetStandardCertificateQuery { CertificateId = certificateId });

            if (result == null)
                return null;

            var viewModel = new CertificateStandardViewModel
            {
                CertificateId = certificateId,
                FamilyName = result.FamilyName,
                GivenNames = result.GivenNames,
                Uln = result.Uln,
                CertificateType = Enum.TryParse<CertificateType>(result.CertificateType, out var parsed) ? parsed : CertificateType.Unknown,
                CertificateReference = result.CertificateReference,
                CourseCode = result.CourseCode,
                CourseName = result.CourseName,
                CourseOption = result.CourseOption,
                CourseLevel = result.CourseLevel,
                DateAwarded = result.DateAwarded,
                OverallGrade = result.OverallGrade,
                ProviderName = result.ProviderName,
                Ukprn = result.Ukprn,
                EmployerName = result.EmployerName,
                AssessorName = result.AssessorName,
                StartDate = result.StartDate,
                PrintRequestedAt = result.PrintRequestedAt,
                PrintRequestedBy = result.PrintRequestedBy
            };

            var owned = await _sessionService.GetOwnedCertificatesAsync(_userService.GetGovUkIdentifier());

            viewModel.ShowBackLink = (owned?.Count() ?? 0) > 1;

            return viewModel;
        }

        public async Task<DownloadCertificateViewModel?> GetDownloadFrameworkCertificateViewModelAsync(Guid certificateId)
        {
            var result = await Mediator.Send(new GetFrameworkCertificateQuery { CertificateId = certificateId });

            if (result == null)
                return null;

            if (string.IsNullOrWhiteSpace(result.FamilyName)
                || string.IsNullOrWhiteSpace(result.GivenNames)
                || string.IsNullOrWhiteSpace(result.CourseName)                
                || result.DateAwarded == null
                || result.CourseLevel == null
                || string.IsNullOrWhiteSpace(result.FrameworkCertificateNumber))
            {
                throw new InvalidOperationException($"Certificate {certificateId} is missing required data.");
            }

            var viewModel = new DownloadCertificateViewModel
            {               
                FamilyName = result.FamilyName!,
                GivenNames = result.GivenNames!,
                CourseName = result.CourseName!,
                CourseOption = result.CourseOption, 
                CourseLevel = result.CourseLevel,
                DateAwarded = result.DateAwarded.Value,
                CertificateNumber = result.FrameworkCertificateNumber,
                CertificateType = CertificateType.Framework
            };

            return viewModel;
        }

        public async Task<CertificateFrameworkViewModel?> GetCertificateFrameworkViewModel(Guid certificateId)
        {
            var result = await Mediator.Send(new GetFrameworkCertificateQuery { CertificateId = certificateId });

            if (result == null)
                return null;

            var viewModel = new CertificateFrameworkViewModel
            {
                CertificateId = certificateId,
                FamilyName = result.FamilyName,
                GivenNames = result.GivenNames,
                Uln = result.Uln,
                CertificateType = Enum.TryParse<CertificateType>(result.CertificateType, out var parsed) ? parsed : CertificateType.Unknown,
                CertificateReference = result.CertificateReference,
                FrameworkCertificateNumber = result.FrameworkCertificateNumber,
                CourseCode = result.CourseCode,
                CourseName = result.CourseName,
                CourseOption = result.CourseOption,
                CourseLevel = result.CourseLevel,
                DateAwarded = result.DateAwarded,
                OverallGrade = result.OverallGrade,
                ProviderName = result.ProviderName,
                Ukprn = result.Ukprn,
                EmployerName = result.EmployerName,
                AssessorName = result.AssessorName,
                StartDate = result.StartDate,
                PrintRequestedAt = result.PrintRequestedAt,
                PrintRequestedBy = result.PrintRequestedBy,
                QualificationsAndAwardingBodies = result.QualificationsAndAwardingBodies,
                DeliveryInformation = result.DeliveryInformation
            };

            var owned = await _sessionService.GetOwnedCertificatesAsync(_userService.GetGovUkIdentifier());

            viewModel.ShowBackLink = (owned?.Count() ?? 0) > 1;

            return viewModel;
        }

        [ExcludeFromCodeCoverage]
        public async Task<byte[]> GenerateCertificateAsync(DownloadCertificateViewModel model)
        {
            byte[]? templateBytes = null;
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [FullName] = model.FullName,                          
                [AwardedOn] = model.DateAwarded.ToString(DateFormat, CultureInfo.InvariantCulture) ?? string.Empty,
                [CertificateNumber] = model.CertificateNumber
            };

            if (model.CertificateType == CertificateType.Standard)
            {
                templateBytes = model.CoronationEmblem ? await _blob.GetBlobBytesAsync(_digitalCertificatesWebConfiguration.ContainerName, _digitalCertificatesWebConfiguration.GreenStandardTemplateBlobName)
                                                        : await _blob.GetBlobBytesAsync(_digitalCertificatesWebConfiguration.ContainerName, _digitalCertificatesWebConfiguration.StandardTemplateBlobName);
                values.Add(AchievedGrade, model.OverallGrade);
                values.Add(PassedInfo, string.Join(Environment.NewLine,
                    new[]
                    {
                model.CourseName,
                model.CourseOption,
                $"{Level} {model.CourseLevel}"
                    }.Where(x => !string.IsNullOrWhiteSpace(x))));
            }
            else if (model.CertificateType == CertificateType.Framework)
            {
                templateBytes = await _blob.GetBlobBytesAsync(_digitalCertificatesWebConfiguration.ContainerName, _digitalCertificatesWebConfiguration.FrameworkTemplateBlobName);
                values.Add(PassedInfo, string.Join(Environment.NewLine,
                        new[]
                        {
                model.CourseName,
                model.CourseOption,
                $"{model.CourseLevel}  {Level}"
                        }.Where(x => !string.IsNullOrWhiteSpace(x))));
            }

            if (templateBytes is null)
            {
                throw new InvalidOperationException("Template bytes were not loaded.");
            }
                       
            var output = await CreatePDFMemoryStream(templateBytes, values);
            return output.ToArray();
        }

        [ExcludeFromCodeCoverage]
        private async Task<MemoryStream> CreatePDFMemoryStream(byte[] templateBytes, Dictionary<string, string> values)
        {
            await _asposeLicenseService.GetAsposeLicense();

            var templateStream = new MemoryStream(templateBytes);
            var document = new Document(templateStream);
            var fields = document.Form.Fields.Cast<Field>().ToList();

            ValidateRequiredFields(fields, values.Keys);

            foreach (var kv in values)
            {
                var field = FindField(fields, kv.Key);

                if (field is TextBoxField textBox)
                {
                    textBox.Value = kv.Value ?? string.Empty;
                    textBox.ReadOnly = true;
                }
                else
                {
                    field.Value = kv.Value ?? string.Empty;
                }
            }

            document.Form.Flatten();
            var output = new MemoryStream();
            document.Encrypt(
                userPassword: "",
                ownerPassword: _digitalCertificatesWebConfiguration.MasterPassword,
                permissions: Permissions.PrintDocument,
                cryptoAlgorithm: CryptoAlgorithm.AESx128);

            document.Save(output);
            return output;
        }

        private static void ValidateRequiredFields(IEnumerable<Field> fields, IEnumerable<string> requiredFieldNames)
        {
            var missingFields = requiredFieldNames
                .Where(required => FindField(fields, required) is null)
                .ToList();

            if (missingFields.Count != 0)
            {
                throw new InvalidOperationException(
                    $"The PDF template is missing required field(s): {string.Join(", ", missingFields)}");
            }
        }

        private static Field? FindField(IEnumerable<Field> fields, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            return fields.FirstOrDefault(f =>
                string.Equals(f.PartialName, key, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f.FullName, key, StringComparison.OrdinalIgnoreCase));
        }
    }
}