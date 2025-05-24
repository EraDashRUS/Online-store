using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.CartItem;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Product;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.User;
using OnlineStore.BusinessLogic.StaticLogic.Settings;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;
using System.Security.Authentication;
using System.Text;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    /// <summary>
    /// Сервис для управления пользователями: регистрация, получение, аутентификация.
    /// </summary>
    public class UserService(
        ApplicationDbContext context,
        IRepository<User> userRepository,
        IOptions<AdminSettings> adminSettings) : IUserService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IRepository<User> _userRepository = userRepository;
        private readonly List<string> _adminEmails = adminSettings.Value.AdminEmails;

        /// <summary>
        /// Регистрирует нового пользователя. Генерирует корзину по умолчанию.
        /// </summary>
        /// <param name="userDto">Данные пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о созданном пользователе</returns>
        /// <exception cref="ArgumentException">Пользователь с таким email уже существует</exception>
        public async Task<UserResponseDto> CreateUserAsync(UserCreateDto userDto, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await EnsureUserEmailUnique(userDto.Email, cancellationToken);

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

        private async Task EnsureUserEmailUnique(string email, CancellationToken cancellationToken)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email, cancellationToken))
                throw new ArgumentException("Пользователь с таким email уже существует");
        }

        /// <summary>
        /// Получает пользователя по идентификатору.
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о пользователе</returns>
        /// <exception cref="NotFoundException">Пользователь не найден</exception>
        public async Task<UserResponseDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user == null)
                throw new NotFoundException("Пользователь не найден");

            return ConvertToDto(user);
        }

        /// <summary>
        /// Аутентифицирует пользователя по email и паролю.
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="password">Пароль</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о пользователе</returns>
        /// <exception cref="AuthenticationException">Неверные учетные данные</exception>
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
        /// Проверяет соответствие пароля хешу.
        /// </summary>
        private static bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

        /// <summary>
        /// Преобразует пользователя в DTO.
        /// </summary>
        private static UserResponseDto ConvertToDto(User user)
        {
            if (user == null) return null;

            return new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Phone = user.Phone ?? string.Empty,
                Address = user.Address ?? string.Empty,
                ActiveCarts = GetActiveCarts(user)
            };
        }

        private static List<CartDto> GetActiveCarts(User user)
        {
            if (user.Carts == null)
                return new List<CartDto>();

            return user.Carts
                .Where(c => c != null && c.Status == "Active")
                .Select(ConvertCartToDto)
                .ToList();
        }

        private static CartDto ConvertCartToDto(Cart c)
        {
            return new CartDto
            {
                Id = c.Id,
                Status = c.Status ?? string.Empty,
                Items = GetCartItems(c)
            };
        }

        private static List<CartItemDto> GetCartItems(Cart c)
        {
            if (c.CartItems == null)
                return new List<CartItemDto>();

            return c.CartItems
                .Where(ci => ci != null)
                .Select(ConvertCartItemToDto)
                .ToList();
        }

        private static CartItemDto ConvertCartItemToDto(CartItem ci)
        {
            return new CartItemDto
            {
                Id = ci.Id,
                Quantity = ci.Quantity,
                ProductId = ci.ProductId,
                Product = ci.Product != null ? new ProductBriefDto
                {
                    Name = ci.Product.Name,
                    Price = ci.Product.Price,
                    Description = ci.Product.Description
                } : null
            };
        }

        /// <summary>
        /// Хеширует пароль PBKDF2 с фиксированной солью.
        /// </summary>
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