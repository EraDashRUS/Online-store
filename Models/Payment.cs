namespace OnlineStore.Models
{
    namespace OnlineStore.Models
    {
        public class Payment
        {
            public int Id { get; set; }      
            public string Status { get; set; }
            public DateTime PaymentDate { get; set; }

        
            public Order Order { get; set; } 
        }
    }
}
