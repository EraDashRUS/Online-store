namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class CartBriefDto
    {
        public int Id { get; set; }
        public int ItemsCount { get; set; } // Количество товаров в корзине
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }

    }
}
