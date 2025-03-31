namespace OnlineStore.Models
{
    public class Cart
    {
        public int Id { get; set; }

        
        public int UserId { get; set; }
        public User User { get; set; }

        
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public Order Order { get; set; }
    }
}
