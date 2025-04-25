namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class CartItemResponseDto
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => ProductPrice * Quantity;
        // Добавляем статус доступности товара
        public bool IsAvailable { get; set; } = true;
    }
}