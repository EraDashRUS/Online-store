using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Data;
using OnlineStore.Models;

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
        public async Task<ActionResult<UserResponseDto>> CreateUser(UserCreateDto userDto)
        {
            // Создаем пользователя
            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                PasswordHash = HashPassword(userDto.Password),
                Phone = userDto.Phone,
                Address = userDto.Address
            };

            // Создаем корзину
            user.Cart = new Cart
            {
                UserId = user.Id,
                CartItems = new List<CartItem>()
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ConvertToDto(user));
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

        // Остальные ваши методы остаются без изменений
        // ...

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
            return ("Пока нету");
        }
    }
}
