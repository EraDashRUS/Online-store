namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    /// <summary>
    /// DTO для ответа с информацией о товаре
    /// </summary>
    public class ProductResponseDto
    {
        /// <summary>
        /// Идентификатор товара
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Наименование товара
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Цена товара
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Количество товара на складе
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Описание товара
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Дата создания товара
        /// </summary>
        public string CreatedAt { get; set; }

        /// <summary>
        /// Дата последнего обновления товара
        /// </summary>
        public string UpdatedAt { get; set; }
    }
}