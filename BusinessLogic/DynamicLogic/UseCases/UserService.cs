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
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<User> _userRepository;
        private readonly List<string> _adminEmails;

        public UserService(
            ApplicationDbContext context,
            IRepository<User> userRepository,
            IOptions<AdminSettings> adminSettings)
        {
            _context = context;
            _userRepository = userRepository;
            _adminEmails = adminSettings.Value.AdminEmails;
        }

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

        public async Task<UserResponseDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user == null)
                throw new NotFoundException("Пользователь не найден");

            return ConvertToDto(user);
        }

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

        private static bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

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