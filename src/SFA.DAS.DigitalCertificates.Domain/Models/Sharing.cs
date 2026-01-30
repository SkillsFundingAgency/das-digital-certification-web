using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Domain.Models
{
    public class Sharing
    {
        public Guid SharingId { get; set; }
        public int SharingNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid LinkCode { get; set; }
        public DateTime ExpiryTime { get; set; }

        public List<DateTime>? SharingAccess { get; set; }
        public List<SharingEmail>? SharingEmails { get; set; }

        public static implicit operator Sharing?(SharingItem? source)
        {
            if (source is null)
            {
                return null;
            }

            return new Sharing
            {
                SharingId = source.SharingId,
                SharingNumber = source.SharingNumber,
                CreatedAt = source.CreatedAt,
                LinkCode = source.LinkCode,
                ExpiryTime = source.ExpiryTime,
                SharingAccess = source.SharingAccess?.ToList(),
                SharingEmails = source.SharingEmails != null
                    ? source.SharingEmails
                        .Where(e => e is not null)
                        .Select(e => (SharingEmail)e!)
                        .ToList()
                    : null
            };
        }
    }
}
