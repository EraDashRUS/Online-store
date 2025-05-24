using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineStore.Storage.Models
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

        /// <summary>
        /// Статус корзины (например, "Active", "Pending", "Completed").
        /// </summary>
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Идентификатор пользователя, которому принадлежит корзина.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Пользователь, владеющий корзиной.
        /// </summary>
        [JsonIgnore]
        [ForeignKey("UserId")]
        public User User { get; set; }

        /// <summary>
        /// Список товаров, находящихся в корзине.
        /// </summary>
        public List<CartItem> CartItems { get; set; } = new();

        /// <summary>
        /// Заказ, связанный с данной корзиной (если оформлен).
        /// </summary>
        [JsonIgnore]
        public Order? Order { get; set; }
    }
}
