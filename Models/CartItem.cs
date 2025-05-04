namespace OnlineStore.Models
{
    /// <summary>
    /// Представляет предмет в корзине.
    /// </summary>
    public class CartItem
    {
        /// <summary>
        /// Идентификатор доставки.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Количество продукта в корзине.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Идентификатор корзины.
        /// </summary>
        public int CartId { get; set; }

        /// <summary>
        /// Идентификатор продукта.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Навигационные свойства.
        /// </summary>
        public Cart Cart { get; set; }
        public Product Product { get; set; }
    }
}