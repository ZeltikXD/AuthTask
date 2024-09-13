using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace AuthTask.Shared.HtmlHelpers
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent PageLinks(this IHtmlHelper htmlHelper, PageInfo pageInfo, Func<int, string?> PageUrl)
        {
            var navTag = GetNavTag();
            var pagingTags = GetUlTag();
            navTag.InnerHtml.AppendHtml(pagingTags);

            // Prev Page
            if (pageInfo.CurrentPage > 1)
            {
                pagingTags.InnerHtml.AppendHtml(GetTagString("Prev", PageUrl(pageInfo.CurrentPage - 1) ?? string.Empty, false));
            }

            // Determine range of pages to show
            int totalPagesToShow = 5;
            int startPage = Math.Max(1, pageInfo.CurrentPage - 2);
            int endPage = Math.Min(pageInfo.LastPage, startPage + totalPagesToShow - 1);

            if (startPage > 1)
            {
                pagingTags.InnerHtml.AppendHtml(GetTagString("1", PageUrl(1) ?? string.Empty, false));
                if (startPage > 2)
                {
                    pagingTags.InnerHtml.AppendHtml("<li>...</li>");
                }
            }

            // Number of pages
            for (int i = startPage; i <= endPage; i++)
            {
                bool isActive = i == pageInfo.CurrentPage;
                pagingTags.InnerHtml.AppendHtml(GetTagString(i.ToString(), PageUrl(i) ?? string.Empty, isActive));
            }

            // Show ellipsis if it is not in the last page
            if (endPage < pageInfo.LastPage)
            {
                if (endPage < pageInfo.LastPage - 1)
                {
                    pagingTags.InnerHtml.AppendHtml("<li>...</li>");
                }
                pagingTags.InnerHtml.AppendHtml(GetTagString(pageInfo.LastPage.ToString(), PageUrl(pageInfo.LastPage) ?? string.Empty, false));
            }

            // Next Page
            if (pageInfo.CurrentPage < pageInfo.LastPage)
            {
                pagingTags.InnerHtml.AppendHtml(GetTagString("Next", PageUrl(pageInfo.CurrentPage + 1) ?? string.Empty, false));
            }

            return htmlHelper.Raw(navTag.TagToString());
        }

        private static string GetTagString(string innerHtml, string hrefValue, bool isActive)
        {
            TagBuilder tagLi = new("li");
            tagLi.AddCssClass("page-item");
            TagBuilder tag = new("a"); // Construct an <a> tag
            tagLi.InnerHtml.AppendHtml(tag);
            tag.AddCssClass("page-link");
            tag.AddCssClass(isActive ? "active" : string.Empty);
            if (isActive) tag.MergeAttribute("aria-current", "page");
            tag.MergeAttribute("class", "anchorstyle");
            tag.MergeAttribute("href", hrefValue);
            tag.InnerHtml.Append(" " + innerHtml + "  ");
            return tagLi.TagToString();
        }

        private static TagBuilder GetNavTag()
        {
            TagBuilder tag = new("nav");
            tag.Attributes.Add("aria-label", "Page navigation");
            return tag;
        }

        private static TagBuilder GetUlTag()
        {
            TagBuilder tag = new("ul");
            tag.AddCssClass("pagination");
            return tag;
        }

        private static string TagToString(this TagBuilder tag)
        {
            using var sw = new StringWriter();
            tag.WriteTo(sw, HtmlEncoder.Default);
            return sw.ToString();
        }
    }
}
