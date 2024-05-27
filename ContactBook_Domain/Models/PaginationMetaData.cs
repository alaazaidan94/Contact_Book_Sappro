namespace ContactBook_Domain.Models
{
    public class PaginationMetaData
    {
        public PaginationMetaData(int totalItemCount, int pageSize, int currentPage)
        {
            TotalItemCount = totalItemCount;
            TotalPageCount = (int) Math.Ceiling(totalItemCount / (double) pageSize);
            PageSize = pageSize;
            CurrentPage = currentPage;
        }

        public int TotalPageCount { get; set; }
        public int TotalItemCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
