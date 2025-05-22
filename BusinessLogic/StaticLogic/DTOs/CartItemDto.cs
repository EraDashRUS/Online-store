namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }

        // Дополнительные свойства товара при необходимости
        public string ProductDescription { get; set; }
    }
}
