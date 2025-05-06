namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    /// <summary>
    /// DTO для обновления элемента корзины
    /// </summary>
    public class CartItemUpdateDto
    {
        /// <summary>
        /// Идентификатор элемента корзины
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Количество товара
        /// </summary>
        public int Quantity { get; set; }
    }
}