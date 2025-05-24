using System.ComponentModel.DataAnnotations;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class OrderCreateDto
    {
        [Required]
        public string Status { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        public string DeliveryAddress { get; set; }

        public int UserId { get; set; }
        public int CartId { get; set; }

        // Для Delivery
        public string DeliveryStatus { get; set; }
        public DateTime DeliveryDate { get; set; }

        // Для Payment
        public string PaymentStatus { get; set; }
    }
}
