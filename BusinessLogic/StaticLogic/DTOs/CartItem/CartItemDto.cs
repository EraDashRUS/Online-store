using OnlineStore.BusinessLogic.StaticLogic.DTOs.Product;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.CartItem
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public ProductBriefDto Product { get; set; }
    }
}
