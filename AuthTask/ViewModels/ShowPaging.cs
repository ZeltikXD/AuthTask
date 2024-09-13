using AuthTask.Shared.HtmlHelpers;

namespace AuthTask.ViewModels
{
    public sealed class ShowPaging<TItem>
    {
        public IEnumerable<TItem> DisplayResult { get; set; } = [];

        public PageInfo PageInfo { get; set; } = null!;
    }
}
