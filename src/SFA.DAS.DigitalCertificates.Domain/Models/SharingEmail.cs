using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.DigitalCertificates.Domain.Models
{
    public class SharingEmail
    {
        public Guid SharingEmailId { get; set; }
        public required string EmailAddress { get; set; }
        public Guid EmailLinkCode { get; set; }
        public DateTime SentTime { get; set; }
        public List<DateTime>? SharingEmailAccess { get; set; }

        public static implicit operator SharingEmail?(SharingEmailItem? source)
        {
            if (source == null)
            {
                return null;
            }

            return new SharingEmail
            {
                SharingEmailId = source.SharingEmailId,
                EmailAddress = source.EmailAddress,
                EmailLinkCode = source.EmailLinkCode,
                SentTime = source.SentTime,
                SharingEmailAccess = source.SharingEmailAccess?.ToList()
            };
        }
    }
}
