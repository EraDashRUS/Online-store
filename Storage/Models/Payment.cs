using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineStore.Storage.Models
{
    /// <summary>
    /// Представляет платёж, связанный с заказом в интернет-магазине.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Уникальный идентификатор платежа.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Текущий статус платежа (например, "Ожидает оплаты", "Оплачен", "Отклонён").
        /// </summary>
        [Required]
        public string Status { get; set; }

        /// <summary>
        /// Сумма платежа в рублях.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Дата и время проведения платежа.
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Идентификатор связанного заказа.
        /// </summary>
        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// Заказ, к которому относится данный платёж.
        /// Не сериализуется в JSON.
        /// </summary>
        [JsonIgnore]
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}