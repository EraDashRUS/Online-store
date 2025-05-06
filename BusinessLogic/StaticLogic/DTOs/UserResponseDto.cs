namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    /// <summary>
    /// DTO для ответа с информацией о пользователе
    /// </summary>
    public class UserResponseDto
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Электронная почта пользователя
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Номер телефона пользователя
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Адрес пользователя
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Идентификатор текущей корзины пользователя
        /// </summary>
        public int CartId { get; set; }

        /// <summary>
        /// Список идентификаторов всех корзин пользователя
        /// </summary>
        public List<int> CartIds { get; set; }
    }
}