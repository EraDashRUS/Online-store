namespace OnlineStore.Storage.Models
{
    /// <summary>
    /// Представляет товар в корзине пользователя.
    /// </summary>
    public class CartItem
    {
        /// <summary>
        /// Уникальный идентификатор позиции в корзине.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Количество выбранного товара.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Идентификатор корзины, к которой относится эта позиция.
        /// </summary>
        public int CartId { get; set; }

        /// <summary>
        /// Идентификатор товара.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Корзина, к которой относится эта позиция.
        /// </summary>
        public Cart Cart { get; set; }

        /// <summary>
        /// Товар, добавленный в корзину.
        /// </summary>
        public Product Product { get; set; }
    }
}