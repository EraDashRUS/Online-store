using System.ComponentModel.DataAnnotations;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    /// <summary>
    /// DTO для авторизации пользователя
    /// </summary>
    public class UserLoginDto
    {
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
        public string Password { get; set; }
    }
}
