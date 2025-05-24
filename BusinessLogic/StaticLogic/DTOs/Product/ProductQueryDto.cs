namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.Product
{
    public class ProductQueryDto
    {
        public string? SearchTerm { get; set; }  
        public decimal? MinPrice { get; set; }   
        public decimal? MaxPrice { get; set; }  
        public bool? InStock { get; set; }     
        public string? SortBy { get; set; }   
        public bool SortDescending { get; set; } 
    }
}