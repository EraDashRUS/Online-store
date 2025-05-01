using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Data;
using OnlineStore.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace OnlineStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public UsersController(
            IUserService userService,
            ApplicationDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [HttpPost]
        [HttpPost]
        public async Task<UserResponseDto> CreateUserAsync(UserCreateDto userDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Заполняем все обязательные поля User
                var user = new User
                {
                    FirstName = userDto.FirstName,  // Обязательно
                    LastName = userDto.LastName,    // Обязательно
                    Email = userDto.Email,         // Обязательно
                    PasswordHash = HashPassword(userDto.Password), // Обязательно
                    Phone = userDto.Phone,
                    Address = userDto.Address// Остальные поля (если есть)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // Теперь FirstName не будет NULL

                // 2. Создаём Cart
                var cart = new Cart { UserId = user.Id };
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

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            return ConvertToDto(user);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            return await _context.Users
                .Include(u => u.Cart)
                .Select(u => ConvertToDto(u))
                .ToListAsync();
        }

        // PUT: api/users/5 (обновление)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto userDto)
        {
            if (id != userDto.Id)
                return BadRequest("ID in URL and body do not match.");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Обновляем только изменяемые поля
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            user.Phone = userDto.Phone;
            user.Address = userDto.Address;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/users/email?email=test@example.com (поиск по email)
        [HttpGet("email")]
        public async Task<ActionResult<UserResponseDto>> GetUserByEmail([EmailAddress] string email)
        {
            var user = await _context.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return NotFound();

            return ConvertToDto(user);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(UserRegisterDto registerDto)
        {
            // Проверка, что email не занят
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                return Conflict("Пользователь с таким email уже существует.");

            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password), // Хешируем пароль
                Phone = registerDto.Phone,
                Address = registerDto.Address
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Создаем корзину для пользователя
            var cart = new Cart { UserId = user.Id };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ConvertToDto(user));
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login(UserLoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                return Unauthorized("Неверный email или пароль.");

            return ConvertToDto(user);
        }

        // Метод для проверки пароля
        private bool VerifyPassword(string password, string storedHash)
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
