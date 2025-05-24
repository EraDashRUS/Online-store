using System.Text.Json.Serialization;

namespace OnlineStore.Storage.Models
{
    /// <summary>
    /// Представляет информацию о доставке, связанную с заказом в интернет-магазине.
    /// </summary>
    public class Delivery
    {
        /// <summary>
        /// Уникальный идентификатор доставки.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Текущий статус доставки (например, "Ожидает отправки", "В пути", "Доставлено").
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Дата и время, когда должна быть осуществлена доставка.
        /// </summary>
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        /// Идентификатор связанного заказа.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Заказ, к которому относится данная доставка.
        /// </summary>
        public Order? Order { get; set; }
    }
}
