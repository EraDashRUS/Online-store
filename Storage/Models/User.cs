using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Storage.Models
{
    /// <summary>
    /// Представляет пользователя интернет-магазина.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Уникальный идентификатор пользователя.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя пользователя. Обязательное поле.
        /// </summary>
        [Required, StringLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия пользователя. Обязательное поле.
        /// </summary>
        [Required, StringLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// Электронная почта для входа и связи. Должна быть уникальной.
        /// </summary>
        [Required, EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Хэш-пароль пользователя. Обязательное поле.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Номер телефона пользователя.
        /// </summary>
        [StringLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Адрес пользователя.
        /// </summary>
        [StringLength(200)]
        public string? Address { get; set; }

        /// <summary>
        /// Список корзин пользователя.
        /// </summary>
        public List<Cart> Carts { get; set; } = [];

        /// <summary>
        /// Навигационные свойства
        /// </summary>
        public Cart Cart { get; set; }
        public List<Order> Orders { get; set; } = [];
    }
}