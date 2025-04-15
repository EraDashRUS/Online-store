namespace OnlineStore.DTOs
{
    public class CartItemCreateDto
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
