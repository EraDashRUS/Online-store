namespace OnlineStore.Storage.Models
{
    /// <summary>
    /// Представляет товар (продукт) интернет-магазина.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Уникальный идентификатор товара.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название товара.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание товара.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Цена товара в рублях.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Количество товара, доступное на складе.
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Список позиций корзины, в которых содержится данный товар (навигационное свойство).
        /// </summary>
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
