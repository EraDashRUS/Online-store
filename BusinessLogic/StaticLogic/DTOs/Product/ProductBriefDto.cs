using System.Text.Json.Serialization;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.CartItem;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.Product

{
    public class ProductBriefDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        [JsonIgnore]
        public List<CartItemDto>? CartItems { get; set; }
    }
}
