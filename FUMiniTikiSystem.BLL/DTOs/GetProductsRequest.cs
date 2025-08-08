namespace FUMiniTikiSystem.BLL.DTOs
{
    public class GetProductsRequest : PagingRequestParam
    {
        public string? SearchTerm { get; set; }
        public int? CategoryID { get; set; }
        public bool? IsActive { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
