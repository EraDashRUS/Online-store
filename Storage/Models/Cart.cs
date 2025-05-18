using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineStore.Storage.Models
{
    /// <summary>
    /// Представляет корзину покупок пользователя.
    /// </summary>
    public class Cart
    {
        public int Id { get; set; }
        public string Status { get; set; } = "Active";
        public int UserId { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public User User { get; set; }


        public List<CartItem> CartItems { get; set; } = new();

        [JsonIgnore]
        public Order? Order { get; set; }
    }
}
