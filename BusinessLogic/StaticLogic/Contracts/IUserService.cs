using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using System.Threading;
using System.Threading.Tasks;

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
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO созданного пользователя</returns>
        /// <exception cref="ArgumentException">Пользователь с таким email уже существует</exception>
        Task<UserResponseDto> CreateUserAsync(UserCreateDto userDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO пользователя</returns>
        /// <exception cref="NotFoundException">Пользователь не найден</exception>
        Task<UserResponseDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Аутентифицирует пользователя
        /// </summary>
        /// <exception cref="AuthenticationException">Неверный email или пароль</exception>
        Task<UserResponseDto> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);

    }
}
