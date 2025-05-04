namespace OnlineStore.Models
{
    namespace OnlineStore.Models
    {
        /// <summary>
        /// Оплата.
        /// </summary>
        public class Payment
        {
            /// <summary>
            /// Идентификатор оплаты.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Статус оплаты.
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// Дата оплаты
            /// </summary>
            public DateTime PaymentDate { get; set; }

            /// <summary>
            /// Заказ (навигационное свойство).
            /// </summary>
            public Order Order { get; set; } 
        }
    }
}
