using OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Product;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.CartItem
{

    public class CartItemResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        public CartBriefDto Cart { get; set; }
        public int Quantity { get; set; }

        public ProductBriefDto Product { get; set; }

    }
}