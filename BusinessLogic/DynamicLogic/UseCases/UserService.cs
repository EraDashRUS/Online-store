using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.BusinessLogic.StaticLogic.Settings;
using OnlineStore.Data;
using OnlineStore.Models;
using System.Security.Authentication;
using System.Text;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    /// <summary>
    /// Сервис для работы с пользователями
    /// </summary>
    /// <remarks>
    /// Инициализирует новый экземпляр класса UserService
    /// </remarks>
    /// <param name="context">Контекст базы данных</param>
    /// <param name="userRepository">Репозиторий пользователей</param>
    /// <param name="adminSettings">Настройки администраторов</param>
    public class UserService(
        ApplicationDbContext context,
        IRepository<User> userRepository,
        IOptions<AdminSettings> adminSettings) : IUserService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IRepository<User> _userRepository = userRepository;
        private readonly List<string> _adminEmails = adminSettings.Value.AdminEmails;

        /// <summary>
        /// Создает нового пользователя
        /// </summary>
        /// <param name="userDto">DTO с данными пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO созданного пользователя</returns>
        /// <exception cref="ArgumentException">Возникает, если пользователь с таким email уже существует</exception>
        public async Task<UserResponseDto> CreateUserAsync(UserCreateDto userDto, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == userDto.Email, cancellationToken))
                    throw new ArgumentException("Пользователь с таким email уже существует");

                var user = new User
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    PasswordHash = HashPassword(userDto.Password),
                    Phone = userDto.Phone,
                    Address = userDto.Address,
                    Carts = [new Cart()]
                };

                await _userRepository.AddAsync(user, cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return ConvertToDto(user);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Получает пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO пользователя</returns>
        /// <exception cref="NotFoundException">Возникает, если пользователь не найден</exception>
        public async Task<UserResponseDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            return user == null ? throw new NotFoundException("Пользователь не найден") : ConvertToDto(user);
        }

        /// <summary>
        /// Аутентифицирует пользователя
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO аутентифицированного пользователя</returns>
        /// <exception cref="AuthenticationException">Возникает при неверных учетных данных</exception>
        public async Task<UserResponseDto> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
                throw new AuthenticationException("Неверный email или пароль");

            var userDto = ConvertToDto(user);
            userDto.IsAdmin = _adminEmails.Contains(user.Email.ToLower());

            return userDto;
        }

        /// <summary>
        /// Проверяет соответствие пароля хешу
        /// </summary>
        /// <param name="password">Проверяемый пароль</param>
        /// <param name="storedHash">Сохраненный хеш</param>
        /// <returns>true если пароль верный, иначе false</returns>
        private static bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

        /// <summary>
        /// Преобразует сущность пользователя в DTO
        /// </summary>
        /// <param name="user">Сущность пользователя</param>
        /// <returns>DTO пользователя</returns>
        private static UserResponseDto ConvertToDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                CartId = user.Cart?.Id ?? 0
            };
        }

        /// <summary>
        /// Хеширует пароль
        /// </summary>
        /// <param name="password">Исходный пароль</param>
        /// <returns>Хеш пароля</returns>
        private static string HashPassword(string password)
        {
            byte[] salt = Encoding.ASCII.GetBytes("FIXED_SALT");

            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}