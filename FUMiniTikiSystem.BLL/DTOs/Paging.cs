namespace FUMiniTikiSystem.BLL.DTOs
{
    public abstract class PagingRequestParam
    {
        private const int MaxPageSize = 50;
        private const int DefaultPageSize = 10;
        private const int DefaultPageNumber = 1;

        private int _pageNumber = DefaultPageNumber;
        private int _pageSize = DefaultPageSize;

        public virtual int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? DefaultPageNumber : value;
        }

        public virtual int PageSize
        {
            get => _pageSize;
            set =>
                _pageSize =
                    value > MaxPageSize ? MaxPageSize : (value < 1 ? DefaultPageSize : value);
        }

        public string? OrderBy { get; set; }

        public SortDirection SortDirection { get; set; } = SortDirection.Asc;
    }

    public class PaginatedList<T>
    {
        public List<T> Items { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            Items = items;
            TotalCount = count;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }

    public enum SortDirection
    {
        Asc = 1,
        Desc = 2,
    }
}
