using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Data;
using OnlineStore.Models;
using System.Security.Cryptography;
using System.Text;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<User> _userRepository;

        public UserService(
            ApplicationDbContext context,
            IRepository<User> userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        public async Task<UserResponseDto> CreateUserAsync(UserCreateDto userDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Проверка существования пользователя
                if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                    throw new ArgumentException("Пользователь с таким email уже существует");

                // Создаем пользователя и корзину в одной транзакции
                var user = new User
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    PasswordHash = HashPassword(userDto.Password),
                    Phone = userDto.Phone,
                    Address = userDto.Address,
                    Carts = new List<Cart> { new Cart() } // Сразу создаем корзину
                };

                _context.Users.Add(user);

                var cart = new Cart { User = user };
                _context.Carts.Add(cart);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ConvertToDto(user);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UserResponseDto> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new NotFoundException("Пользователь не найден");

            return ConvertToDto(user);
        }

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

        private string HashPassword(string password)
        {
            // Используем стандартный метод ASP.NET Core Identity
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: RandomNumberGenerator.GetBytes(128 / 8),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}