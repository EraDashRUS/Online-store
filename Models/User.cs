using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public List<Cart> Carts { get; set; } = new();

        // Навигационные свойства
        public Cart Cart { get; set; }
        public List<Order> Orders { get; set; } = new();
    }
}