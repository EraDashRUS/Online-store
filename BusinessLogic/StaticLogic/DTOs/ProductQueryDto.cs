namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class ProductQueryDto
    {
        public string? SearchTerm { get; set; }  // Поиск по названию и описанию
        public decimal? MinPrice { get; set; }   // Фильтр по минимальной цене
        public decimal? MaxPrice { get; set; }   // Фильтр по максимальной цене
        public bool? InStock { get; set; }       // Только товары в наличии
        public string? SortBy { get; set; }      // Поле для сортировки
        public bool SortDescending { get; set; } // Направление сортировки
    }
}