using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Data;
using OnlineStore.Models;
using System.Text;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    /// <summary>
    /// Сервис для работы с пользователями
    /// </summary>
    public class UserService(
        ApplicationDbContext context,
        IRepository<User> userRepository) : IUserService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IRepository<User> _userRepository = userRepository;

        /// <summary>
        /// Создает нового пользователя
        /// </summary>
        /// <param name="userDto">DTO с данными пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO созданного пользователя</returns>
        /// <exception cref="ArgumentException">Пользователь с таким email уже существует</exception>
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
                    Carts = new List<Cart> { new Cart() }
                };

                _context.Users.Add(user);

                var cart = new Cart { User = user };
                _context.Carts.Add(cart);

                await _context.SaveChangesAsync(cancellationToken);
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
        /// Преобразует сущность пользователя в DTO
        /// </summary>
        /// <param name="user">Сущность пользователя</param>
        /// <returns>DTO пользователя</returns>
        private UserResponseDto ConvertToDto(User user)
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
        /// Хеширует пароль пользователя
        /// </summary>
        /// <param name="password">Исходный пароль</param>
        /// <returns>Хешированный пароль</returns>
        private string HashPassword(string password)
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