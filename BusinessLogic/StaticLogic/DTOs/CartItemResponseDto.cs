namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    /// <summary>
    /// DTO для ответа с информацией об элементе корзины
    /// </summary>
    public class CartItemResponseDto
    {
        /// <summary>
        /// Идентификатор элемента корзины
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор корзины
        /// </summary>
        public int CartId { get; set; }

        /// <summary>
        /// Идентификатор товара
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Наименование товара
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Цена товара
        /// </summary>
        public decimal ProductPrice { get; set; }

        /// <summary>
        /// Количество товара
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Общая стоимость позиции (цена * количество)
        /// </summary>
        public decimal TotalPrice => ProductPrice * Quantity;

        /// <summary>
        /// Признак доступности товара
        /// </summary>
        public bool IsAvailable { get; set; } = true;
    }
}