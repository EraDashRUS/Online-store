namespace OnlineStore.Models
{
    public class Delivery
    {
        public int Id { get; set; }          
        public string Status { get; set; }
        public DateTime DeliveryDate { get; set; }

        
        public Order Order { get; set; }     
    }
}
