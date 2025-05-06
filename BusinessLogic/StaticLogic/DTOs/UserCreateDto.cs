using System.ComponentModel.DataAnnotations;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    /// <summary>
    /// DTO для создания нового пользователя
    /// </summary>
    public class UserCreateDto
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        [Required, StringLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        [Required, StringLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// Электронная почта пользователя
        /// </summary>
        [Required, EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        /// <summary>
        /// Номер телефона пользователя
        /// </summary>
        [StringLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Адрес пользователя
        /// </summary>
        [StringLength(200)]
        public string? Address { get; set; }
    }
}
