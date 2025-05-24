using System.ComponentModel.DataAnnotations;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.Order
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

        public string DeliveryStatus { get; set; }
        public DateTime DeliveryDate { get; set; }

        public string PaymentStatus { get; set; }
    }
}
