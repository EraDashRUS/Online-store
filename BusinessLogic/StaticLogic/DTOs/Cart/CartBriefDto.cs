namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart
{
    public class CartBriefDto
    {
        public int Id { get; set; }
        public int ItemsCount { get; set; } 
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }

    }
}
