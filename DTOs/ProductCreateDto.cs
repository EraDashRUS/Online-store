using System.ComponentModel.DataAnnotations;

namespace OnlineStore.DTOs
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Название товара обязательно")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 100 символов")]
        public string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть положительной")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int StockQuantity { get; set; } = 0;

        [StringLength(500)]
        public string Description { get; set; }
    }
}
