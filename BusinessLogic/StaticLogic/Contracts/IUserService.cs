using OnlineStore.BusinessLogic.StaticLogic.DTOs;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    /// <summary>
    /// Интерфейс сервиса для работы с пользователями
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Создает нового пользователя
        /// </summary>
        /// <param name="userDto">DTO с данными пользователя</param>
        /// <returns>DTO созданного пользователя</returns>
        /// <exception cref="ArgumentException">Пользователь с таким email уже существует</exception>
        Task<UserResponseDto> CreateUserAsync(UserCreateDto userDto);

        /// <summary>
        /// Получает пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>DTO пользователя</returns>
        /// <exception cref="NotFoundException">Пользователь не найден</exception>
        Task<UserResponseDto> GetUserByIdAsync(int id);
    }
}
