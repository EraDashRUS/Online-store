using OnlineStore.BusinessLogic.StaticLogic.DTOs.CartItem;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart
{

    public class CartResponseDto
    {

        public int Id { get; set; }

 
        public int UserId { get; set; }

 
        public List<CartItemResponseDto> Items { get; set; } = new();

        public List<CartItemDto> CartItems { get; set; }


        public decimal TotalPrice { get; set; }


        public string Status { get; set; }
    }
}
