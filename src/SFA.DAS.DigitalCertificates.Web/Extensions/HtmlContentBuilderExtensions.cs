using Microsoft.AspNetCore.Html;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Encodings.Web;

namespace SFA.DAS.DigitalCertificates.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class HtmlContentBuilderExtensions
    {
        public static bool IsNullOrEmpty(this IHtmlContentBuilder htmlContentBuilder)
        {
            using (var writer = new StringWriter())
            {
                htmlContentBuilder.WriteTo(writer, HtmlEncoder.Default);
                var content = writer.ToString();
                return string.IsNullOrEmpty(content);
            }
        }
    }
}