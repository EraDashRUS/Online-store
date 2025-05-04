namespace OnlineStore.Models
{
    /// <summary>
    /// Представляет продукт интернет-магазина.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Идентификатор продукта.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название продукта.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание продукта.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Цена продукта.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Количество продукта на складе.
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Список товаров в корзине (навигационное свойство).
        /// </summary>
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
