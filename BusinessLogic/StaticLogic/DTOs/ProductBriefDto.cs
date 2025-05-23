using System.Text.Json.Serialization;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs

{
    public class ProductBriefDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        [JsonIgnore] // Исключаем циклические ссылки
        public List<CartItemDto>? CartItems { get; set; }
    }
}
