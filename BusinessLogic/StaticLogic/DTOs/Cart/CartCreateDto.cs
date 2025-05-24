using OnlineStore.BusinessLogic.StaticLogic.DTOs.CartItem;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart
{
    public class CartCreateDto
    {
        public int UserId { get; set; } 
        public List<CartItemCreateDto> Items { get; set; }
    }
}
