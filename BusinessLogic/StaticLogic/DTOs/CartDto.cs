namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; } // Добавляем UserId
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalPrice { get; set; } // Добавляем TotalPrice
    }
}