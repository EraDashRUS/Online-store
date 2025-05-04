using OnlineStore.Models.OnlineStore.Models;

namespace OnlineStore.Models
{
    /// <summary>
    /// Представляет заказ в интернет-магазине.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Уникальный идентификатор заказа.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата и время создания заказа.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Текущий статус заказа (например, "Обработка", "Доставлен").
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Общая сумма заказа в рублях.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Адрес доставки заказа.
        /// </summary>
        public string DeliveryAddress { get; set; }

        /// <summary>
        /// Идентификатор связанной доставки.
        /// </summary>
        public int DeliveryId { get; set; }

        /// <summary>
        /// Идентификатор связанного платежа.
        /// </summary>
        public int PaymentId { get; set; }

        /// <summary>
        /// Идентификатор пользователя, оформившего заказ.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Идентификатор корзины, связанной с заказом (может быть null, если корзина удалена).
        /// </summary>
        public int? CartId { get; set; }

        /// <summary>
        /// Навигационное свойство для доставки.
        /// </summary>
        public Delivery Delivery { get; set; }

        /// <summary>
        /// Навигационное свойство для платежа.
        /// </summary>
        public Payment Payment { get; set; }

        /// <summary>
        /// Навигационное свойство для корзины (может быть null).
        /// </summary>
        public Cart Cart { get; set; }
    }
}
