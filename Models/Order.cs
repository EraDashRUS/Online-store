using OnlineStore.Models.OnlineStore.Models;

namespace OnlineStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryAddress { get; set; }

        
        public int DeliveryId { get; set; }  
        public int PaymentId { get; set; }  
        public int UserId { get; set; }  
        public int? CartId { get; set; }  

        
        public Delivery Delivery { get; set; }
        public Payment Payment { get; set; }
        public Cart Cart { get; set; }
    }
}
