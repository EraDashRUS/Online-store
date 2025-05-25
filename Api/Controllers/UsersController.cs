using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.CartItem;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.User;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Product;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using System.Text;

namespace OnlineStore.Controllers
{
    /// <summary>
    /// Контроллер для управления пользователями и их аутентификацией.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(
        IUserService userService,
        ApplicationDbContext context) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Создать нового пользователя и активную корзину.
        /// </summary>
        /// <param name="userDto">Данные пользователя</param>
        /// <returns>Созданный пользователь</returns>
        [HttpPost]
        public async Task<ActionResult<UserDetailDto>> CreateUserAsync(UserCreateDto userDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await CreateUserInternalAsync(userDto);
                var cart = await CreateCartForUserAsync(user.Id);
                await transaction.CommitAsync();
                var createdUser = await LoadUserWithCartsAsync(user.Id);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, ConvertToDetailDto(createdUser));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<User> CreateUserInternalAsync(UserCreateDto userDto)
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
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        private async Task<Cart> CreateCartForUserAsync(int userId)
        {
            var cart = new Cart
            {
                UserId = userId,
                Status = "Active"
            };
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        private async Task<User> LoadUserWithCartsAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Carts)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        /// <summary>
        /// Получить пользователя по идентификатору.
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <returns>Детальная информация о пользователе</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailDto>> GetUserById(int id)
        {
            var user = await _context.Users
                .Include(u => u.Carts)
                    .ThenInclude(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            return ConvertToDetailDto(user);
        }

        /// <summary>
        /// Получить список всех пользователей.
        /// </summary>
        /// <returns>Список пользователей</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<ActionResult<List<UserBriefDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.Carts)
                    .ThenInclude(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                .ToListAsync();

            return users.Select(ConvertToBriefDto).ToList();
        }

        /// <summary>
        /// Обновить данные пользователя.
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="userDto">Обновленные данные</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат операции</returns>
        [Authorize(Policy = "AdminOnly")]
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

            UpdateUserFields(user, userDto);

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

        private static void UpdateUserFields(User user, UserUpdateDto userDto)
        {
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            user.Phone = userDto.Phone;
            user.Address = userDto.Address;
        }

        /// <summary>
        /// Удалить пользователя по идентификатору.
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат операции</returns>
        [Authorize(Policy = "AdminOnly")]
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
        /// Получить пользователя по email.
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о пользователе</returns>
        [Authorize(Policy = "AdminOnly")]
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
        /// Зарегистрировать нового пользователя.
        /// </summary>
        /// <param name="registerDto">Данные для регистрации</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Зарегистрированный пользователь</returns>
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

            var user = await RegisterUserInternalAsync(registerDto, cancellationToken);
            var cart = await RegisterCartForUserAsync(user.Id, cancellationToken);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, ConvertToDto(user));
        }

        private async Task<User> RegisterUserInternalAsync(UserRegisterDto registerDto, CancellationToken cancellationToken)
        {
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
            return user;
        }

        private async Task<Cart> RegisterCartForUserAsync(int userId, CancellationToken cancellationToken)
        {
            var cart = new Cart { UserId = userId };
            await _context.Carts.AddAsync(cart, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return cart;
        }

        /// <summary>
        /// Аутентификация пользователя.
        /// </summary>
        /// <param name="loginDto">Данные для входа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о пользователе</returns>
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

        /// <summary>
        /// Проверить, является ли пользователь администратором.
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True если админ, иначе False</returns>
        [Authorize(Policy = "AdminOnly")]
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
        /// Проверить существование пользователя по ID.
        /// </summary>
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        /// <summary>
        /// Проверить соответствие пароля хешу.
        /// </summary>
        private static bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

        /// <summary>
        /// Преобразовать сущность User в UserResponseDto.
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
                ActiveCarts = user.Carts?
                    .Where(c => c != null && c.Status == "Active")
                    .Select(ConvertCartToDto)
                    .ToList() ?? new List<CartDto>()
            };
        }

        private static CartDto ConvertCartToDto(Cart c)
        {
            return new CartDto
            {
                Id = c.Id,
                Status = c.Status ?? string.Empty,
                Items = c.CartItems?
                    .Where(ci => ci != null)
                    .Select(ConvertCartItemToDto)
                    .ToList() ?? new List<CartItemDto>()
            };
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

        private UserBriefDto ConvertToBriefDto(User user)
        {
            return new UserBriefDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ActiveCarts = user.Carts
                    .Where(c => c.Status == "Active")
                    .Select(ConvertCartToBriefDto)
                    .ToList()
            };
        }

        private static CartBriefDto ConvertCartToBriefDto(Cart c)
        {
            return new CartBriefDto
            {
                Id = c.Id,
                ItemsCount = c.CartItems.Sum(ci => ci.Quantity),
                Status = c.Status,
                TotalPrice = c.CartItems.Sum(ci => ci.Quantity * (ci.Product?.Price ?? 0))
            };
        }

        private UserDetailDto ConvertToDetailDto(User user)
        {
            return new UserDetailDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                ActiveCarts = user.Carts
                    .Where(c => c.Status == "Active")
                    .Select(ConvertCartToDetailDto)
                    .ToList()
            };
        }

        private static CartDto ConvertCartToDetailDto(Cart c)
        {
            return new CartDto
            {
                Id = c.Id,
                Status = c.Status,
                UserId = c.UserId,
                Items = c.CartItems.Select(ConvertCartItemToDetailDto).ToList(),
                TotalPrice = c.CartItems.Sum(ci => ci.Quantity * (ci.Product?.Price ?? 0))
            };
        }

        private static CartItemDto ConvertCartItemToDetailDto(CartItem ci)
        {
            return new CartItemDto
            {
                Id = ci.Id,
                Quantity = ci.Quantity,
                ProductId = ci.ProductId,
                Product = ci.Product != null ? new ProductBriefDto
                {
                    Id = ci.Product.Id,
                    Name = ci.Product.Name,
                    Price = ci.Product.Price,
                    Description = ci.Product.Description,
                    StockQuantity = ci.Product.StockQuantity
                } : null
            };
        }

        /// <summary>
        /// Получить все корзины пользователя.
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns>Список корзин</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{userId}/carts")]
        public async Task<ActionResult<List<CartDto>>> GetUserCarts(int userId)
        {
            var carts = await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .ToListAsync();

            return carts.Select(ConvertCartToFullDto).ToList();
        }

        private static CartDto ConvertCartToFullDto(Cart c)
        {
            return new CartDto
            {
                Id = c.Id,
                Status = c.Status ?? "Pending",
                UserId = c.UserId,
                Items = c.CartItems.Select(ConvertCartItemToFullDto).ToList(),
                TotalPrice = c.CartItems.Sum(ci => ci.Quantity * (ci.Product?.Price ?? 0))
            };
        }

        private static CartItemDto ConvertCartItemToFullDto(CartItem ci)
        {
            return new CartItemDto
            {
                Id = ci.Id,
                Quantity = ci.Quantity,
                ProductId = ci.ProductId,
                Product = ci.Product != null ? new ProductBriefDto
                {
                    Id = ci.Product.Id,
                    Name = ci.Product.Name,
                    Description = ci.Product.Description,
                    Price = ci.Product.Price,
                    StockQuantity = ci.Product.StockQuantity
                } : null
            };
        }

        /// <summary>
        /// Хешировать пароль.
        /// </summary>
        /// <param name="password">Пароль</param>
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