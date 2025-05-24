using OnlineStore.BusinessLogic.StaticLogic.DTOs.CartItem;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart
{
    public class CartDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalPrice { get; set; } 
    }
}