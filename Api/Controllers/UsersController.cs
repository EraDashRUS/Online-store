using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Data;
using OnlineStore.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using System.Text;

namespace OnlineStore.Controllers
{
    /// <summary>
    /// Контроллер для управления пользователями и их аутентификацией
    /// </summary>
    /// <remarks>
    /// Инициализирует новый экземпляр контроллера пользователей
    /// </remarks>
    /// <param name="userService">Сервис для работы с пользователями</param>
    /// <param name="context">Контекст базы данных</param>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(
        IUserService userService,
        ApplicationDbContext context) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly ApplicationDbContext _context = context;

        

        /// <summary>
        /// Создает нового пользователя
        /// </summary>
        /// <param name="userDto">Данные для создания пользователя</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Созданный пользователь</returns>
        /// <response code="201">Пользователь успешно создан</response>
        /// <response code="400">Некорректные данные</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserResponseDto>> CreateUserAsync(
            UserCreateDto userDto,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var user = new User
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    PasswordHash = HashPassword(userDto.Password),
                    Phone = userDto.Phone,
                    Address = userDto.Address
                };

                await _context.Users.AddAsync(user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var cart = new Cart { UserId = user.Id };
                await _context.Carts.AddAsync(cart, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ConvertToDto(user));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Получает пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Данные пользователя</returns>
        /// <response code="200">Пользователь найден</response>
        /// <response code="404">Пользователь не найден</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> GetUser(
            int id,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user == null)
                return NotFound();

            return ConvertToDto(user);
        }

        /// <summary>
        /// Получает список всех пользователей
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Список пользователей</returns>
        /// <response code="200">Успешно возвращен список пользователей</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers(
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Cart)
                .Select(u => ConvertToDto(u))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Обновляет данные пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="userDto">Обновленные данные пользователя</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Данные успешно обновлены</response>
        /// <response code="400">Неверный идентификатор</response>
        /// <response code="404">Пользователь не найден</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(
            int id,
            UserUpdateDto userDto,
            CancellationToken cancellationToken = default)
        {
            if (id != userDto.Id)
                return BadRequest("ID в URL и теле запроса не совпадают");

            var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
            if (user == null)
                return NotFound();

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            user.Phone = userDto.Phone;
            user.Address = userDto.Address;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Удаляет пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Пользователь успешно удален</response>
        /// <response code="404">Пользователь не найден</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(
            int id,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Находит пользователя по email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Данные пользователя</returns>
        /// <response code="200">Пользователь найден</response>
        /// <response code="400">Некорректный email</response>
        /// <response code="404">Пользователь не найден</response>
        [HttpGet("email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> GetUserByEmail(
            [EmailAddress] string email,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user == null)
                return NotFound();

            return ConvertToDto(user);
        }

        /// <summary>
        /// Регистрирует нового пользователя
        /// </summary>
        /// <param name="registerDto">Данные для регистрации</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Зарегистрированный пользователь</returns>
        /// <response code="201">Пользователь успешно зарегистрирован</response>
        /// <response code="400">Некорректные данные</response>
        /// <response code="409">Пользователь с таким email уже существует</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserResponseDto>> Register(
            UserRegisterDto registerDto,
            CancellationToken cancellationToken = default)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email, cancellationToken))
                return Conflict("Пользователь с таким email уже существует.");

            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                Phone = registerDto.Phone,
                Address = registerDto.Address
            };

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var cart = new Cart { UserId = user.Id };
            await _context.Carts.AddAsync(cart, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ConvertToDto(user));
        }

        /// <summary>
        /// Аутентифицирует пользователя
        /// </summary>
        /// <param name="loginDto">Данные для входа</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Данные аутентифицированного пользователя</returns>
        /// <response code="200">Успешная аутентификация</response>
        /// <response code="400">Некорректные данные</response>
        /// <response code="401">Неверные учетные данные</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserResponseDto>> Login(
            UserLoginDto loginDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userService.AuthenticateAsync(
                    loginDto.Email,
                    loginDto.Password,
                    cancellationToken);

                return Ok(user);
            }
            catch (AuthenticationException ex)
            {
                return Unauthorized(ex.Message);
            }
        }


        [HttpGet("is-admin/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> IsAdmin(
            string email,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user == null)
                return NotFound();

            var adminEmails = new List<string> { "admin@example.com" }; 
            return Ok(adminEmails.Contains(email.ToLower()));
        }

        /// <summary>
        /// Проверяет существование пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>True если пользователь существует, иначе False</returns>
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        /// <summary>
        /// Проверяет соответствие пароля хешу
        /// </summary>
        /// <param name="password">Пароль для проверки</param>
        /// <param name="storedHash">Хеш из базы данных</param>
        /// <returns>True если пароль верный, иначе False</returns>
        private static bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

        /// <summary>
        /// Преобразует сущность User в DTO
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
        /// <param name="password">Пароль для хеширования</param>
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