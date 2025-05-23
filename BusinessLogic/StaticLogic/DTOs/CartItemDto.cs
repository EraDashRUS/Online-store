namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public ProductBriefDto Product { get; set; } // Заменяем отдельные поля на объект Product
    }
}
