namespace AuthTask.Shared.HtmlHelpers
{
    /// <summary>
    /// Manage pagination information.
    /// </summary>
    public class PageInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }

        public PageInfo()
        {
            CurrentPage = 1;
        }
        //starting item number in the page
        public int PageStart => (CurrentPage - 1) * ItemsPerPage + 1;
       
        //last item number in the page
        public int PageEnd
        {
            get
            {
                int currentTotal = (CurrentPage - 1) * ItemsPerPage + ItemsPerPage;
                return (currentTotal < TotalItems ? currentTotal : TotalItems);
            }
        }

        public int LastPage => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
    }
}
