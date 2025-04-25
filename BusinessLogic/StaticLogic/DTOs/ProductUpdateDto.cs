using System.ComponentModel.DataAnnotations;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class ProductUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; } // Делаем nullable

        [Range(0.01, double.MaxValue)]
        public decimal? Price { get; set; } // Делаем nullable

        [Range(0, int.MaxValue)]
        public int? StockQuantity { get; set; } // Делаем nullable

        [StringLength(500)]
        public string? Description { get; set; } // Добавляем новое поле
    }
}