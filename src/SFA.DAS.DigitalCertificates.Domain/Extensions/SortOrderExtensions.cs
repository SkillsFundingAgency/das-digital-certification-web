using SFA.DAS.DigitalCertificates.Domain.Types;

namespace SFA.DAS.DigitalCertificates.Domain.Extensions
{
    public static class SortOrderExtensions
    {
        public static SortOrder Reverse(this SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Ascending
                ? SortOrder.Descending
                : SortOrder.Ascending;
        }
    }
}
