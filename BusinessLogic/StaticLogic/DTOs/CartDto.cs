namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
}
