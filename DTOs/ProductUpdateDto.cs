using System.ComponentModel.DataAnnotations;

namespace OnlineStore.DTOs
{
    public class ProductUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }
}