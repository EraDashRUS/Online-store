using System.ComponentModel.DataAnnotations;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    /// <summary>
    /// DTO для обновления информации о товаре
    /// </summary>
    public class ProductUpdateDto
    {
        /// <summary>
        /// Идентификатор товара
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Наименование товара
        /// </summary>
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        /// <summary>
        /// Цена товара
        /// </summary>
        [Range(0.01, double.MaxValue)]
        public decimal? Price { get; set; }

        /// <summary>
        /// Количество товара на складе
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? StockQuantity { get; set; }

        /// <summary>
        /// Описание товара
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }
    }
}