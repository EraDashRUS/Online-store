using System.Collections.Generic;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class CartResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartItemResponseDto> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
    }
}
