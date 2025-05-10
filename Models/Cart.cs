namespace OnlineStore.Models
{
    /// <summary>
    /// Представляет корзину покупок пользователя.
    /// </summary>
    public class Cart
    {
        /// <summary>
        /// Уникальный идентификатор корзины.
        /// </summary>
        public int Id { get; set; }


        public string? Status { get; set; }


        /// <summary>
        /// Идентификатор пользователя, которому принадлежит корзина.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Пользователь, связанный с корзиной (навигационное свойство).
        /// </summary>
        public User User { get; set; }

        

        /// <summary>
        /// Список товаров в корзине (навигационное свойство).
        /// </summary>
        public List<CartItem> CartItems { get; set; }
        /// <summary>
        /// Заказ (навигационное свойство).
        /// </summary>
        public Order Order { get; set; }

        public List<CartItem> Items { get; set; } = new();


    }
}
