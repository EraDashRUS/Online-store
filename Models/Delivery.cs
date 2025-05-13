namespace OnlineStore.Models
{
    /// <summary>
    /// Представляет доставку в интернет-магазине.
    /// </summary>
    public class Delivery
    {
        /// <summary>
        /// Идентификатор доставки.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Статус доставки.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Дата доставки.
        /// </summary>
        public DateTime DeliveryDate { get; set; }


        public int OrderId { get; set; }


        /// <summary>
        /// Заказ (навигационное свойство).
        /// </summary>
        public Order Order { get; set; }     
    }
}
