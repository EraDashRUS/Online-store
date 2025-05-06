using System.ComponentModel.DataAnnotations;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    /// <summary>
    /// DTO для регистрации нового пользователя
    /// </summary>
    public class UserRegisterDto
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        [Required] 
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        [Required] 
        public string LastName { get; set; }

        /// <summary>
        /// Электронная почта пользователя
        /// </summary>
        [Required]
        [EmailAddress] 
        public string Email { get; set; }

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        [Required]
        [MinLength(6)] 
        public string Password { get; set; }

        /// <summary>
        /// Номер телефона пользователя
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Адрес пользователя
        /// </summary>
        public string? Address { get; set; }
    }
}
