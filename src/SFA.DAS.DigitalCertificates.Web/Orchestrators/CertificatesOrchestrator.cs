using Aspose.Pdf;
using Aspose.Pdf.Forms;
using MediatR;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Services;
using System;
using System.Collections.Generic;
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
        private readonly DigitalCertificatesWebConfiguration _digitalCertificatesWebConfiguration;

        public CertificatesOrchestrator(IMediator mediator, 
            ISessionService sessionService, 
            IUserService userService, 
            IBlobService blob,
            DigitalCertificatesWebConfiguration digitalCertificatesPdfConfiguration
            )
            : base(mediator)
        {
            _sessionService = sessionService;
            _userService = userService;
            _blob = blob;
            _digitalCertificatesWebConfiguration = digitalCertificatesPdfConfiguration;
        }

        public async Task<CertificatesListViewModel> GetCertificatesListViewModel()
        {
            return new CertificatesListViewModel
            {
                Certificates = await _sessionService.GetOwnedCertificatesAsync(_userService.GetGovUkIdentifier())
            };
        }

        public async Task<DownloadCertificateViewModel> GetDownloadCertificateViewModelAsync(Guid certificateId)
        {
            var result = await Mediator.Send(new GetStandardCertificateQuery { CertificateId = certificateId });

            if (result == null)
                return null;

            var viewModel = new DownloadCertificateViewModel
            {
                FamilyName = result.FamilyName,
                GivenNames = result.GivenNames,
                StandardName = result.CourseName,
                OptionName = result.CourseOption,
                Level = result.CourseLevel.ToString(),
                Result = result.OverallGrade,
                DateAwarded = result.DateAwarded,
                CertificationNumber = result.CertificateReference,
                CoronationEmblem = result.CoronationEmblem
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

        public async Task<byte[]> GenerateCertificateAsync(DownloadCertificateViewModel model)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Full Name"] = model.FullName,
                ["Passed info"] = string.Join("\n",
                    new[]
                    {
                model.StandardName,
                model.OptionName,
                model.Level
                    }.Where(x => !string.IsNullOrWhiteSpace(x))),
                ["Achieved grade"] = model.Result,
                ["Awarded on"] = model.DateAwarded?.ToString("d MMMM yyyy", CultureInfo.InvariantCulture) ?? string.Empty,
            };

            byte[] templateBytes;

            if (model.CoronationEmblem)
            {
                templateBytes = await _blob.GetBlobBytesAsync(_digitalCertificatesWebConfiguration.GreenStandardTemplateBlobName);
            }
            else
            {
                templateBytes = await _blob.GetBlobBytesAsync(_digitalCertificatesWebConfiguration.StandardTemplateBlobName);
            }

            using var templateStream = new MemoryStream(templateBytes);
            using var document = new Document(templateStream);
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

            if (_digitalCertificatesWebConfiguration.Flatten == true)
            {
                document.Form.Flatten();
            }

            using var output = new MemoryStream();

            if (!string.IsNullOrWhiteSpace(_digitalCertificatesWebConfiguration.MasterPassword))
            {
                document.Encrypt(
                    userPassword: "",
                    ownerPassword: _digitalCertificatesWebConfiguration.MasterPassword,
                    permissions: Aspose.Pdf.Permissions.PrintDocument,
                    cryptoAlgorithm: Aspose.Pdf.CryptoAlgorithm.AESx128);
            }

            document.Save(output);
            return output.ToArray();
        }

        private static void ValidateRequiredFields(IEnumerable<Field> fields, IEnumerable<string> requiredFieldNames)
        {
            var missingFields = requiredFieldNames
                .Where(required => FindField(fields, required) is null)
                .ToList();

            if (missingFields.Any())
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