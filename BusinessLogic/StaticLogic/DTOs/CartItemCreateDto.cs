namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    /// <summary>
    /// DTO для создания элемента корзины
    /// </summary>
    public class CartItemCreateDto
    {
        /// <summary>
        /// Идентификатор корзины
        /// </summary>
        public int CartId { get; set; }

        /// <summary>
        /// Идентификатор товара
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Количество товара. По умолчанию 1
        /// </summary>
        public int Quantity { get; set; } = 1;
    }
}
