using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models
{
    public class CreatePaymentDto
    {
        [Required]
        public string Status { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public int OrderId { get; set; }
    }
}