using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SFA.DAS.DigitalCertificates.Web.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent HiddenForArray<TModel, TProperty>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty[]>> expression)
        {
            var modelExplorer = htmlHelper.MetadataProvider.GetModelExplorerForType(typeof(TProperty[]), expression.Compile().Invoke(htmlHelper.ViewData.Model));
            var modelName = htmlHelper.NameFor(expression).ToString();
            var values = modelExplorer.Model as TProperty[];

            if (values == null)
            {
                return HtmlString.Empty;
            }

            var sb = new StringBuilder();

            for (int i = 0; i < values.Length; i++)
            {
                var name = $"{modelName}[{i}]";
                var value = values[i]?.ToString();
                var hiddenTag = htmlHelper.Hidden(name, value);
                sb.AppendLine(hiddenTag.GetString());
            }

            return new HtmlString(sb.ToString());
        }

        private static string GetString(this IHtmlContent content)
        {
            using (var writer = new System.IO.StringWriter())
            {
                content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                return writer.ToString();
            }
        }
    }
}
