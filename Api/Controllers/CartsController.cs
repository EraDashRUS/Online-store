using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.CartItem;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Product;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// API для управления корзинами пользователей.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Получить список всех корзин с товарами и пользователями.
        /// Только для администратора.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список корзин</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CartResponseDto>>> GetAllCarts(CancellationToken cancellationToken)
        {
            var carts = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .Select(c => MapCartToResponseDto(c))
                .ToListAsync(cancellationToken);

            return Ok(carts);
        }

        /// <summary>
        /// Получить корзину по идентификатору.
        /// </summary>
        /// <param name="id">ID корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные корзины</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartResponseDto>> GetCart(int id, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (cart == null)
                return NotFound();

            var result = MapCartToResponseDto(cart);
            return Ok(result);
        }

        /// <summary>
        /// Обновить данные корзины.
        /// </summary>
        /// <param name="id">ID корзины</param>
        /// <param name="cart">Обновлённая корзина</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutCart(int id, Cart cart, CancellationToken cancellationToken)
        {
            if (id != cart.Id)
                return BadRequest();

            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Создать новую корзину.
        /// </summary>
        /// <param name="dto">Данные для создания корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданная корзина</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CartResponseDto>> CreateCart(
            [FromBody] CartCreateDto dto,
            CancellationToken cancellationToken)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return BadRequest("Пользователь не найден");

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var cart = CreateCartFromDto(dto);
                await _context.Carts.AddAsync(cart, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                var createdCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == cart.Id, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return CreatedAtAction(
                    nameof(GetCart),
                    new { id = cart.Id },
                    MapCartToResponseDto(createdCart));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return StatusCode(500, $"Ошибка при создании корзины: {ex.Message}");
            }
        }

        /// <summary>
        /// Удалить корзину по идентификатору.
        /// </summary>
        /// <param name="id">ID корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCart(int id, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FindAsync(new object[] { id }, cancellationToken);
            if (cart == null)
                return NotFound();

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Проверить, существует ли корзина.
        /// </summary>
        /// <param name="id">ID корзины</param>
        /// <returns>True, если корзина существует</returns>
        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }

        private static CartResponseDto MapCartToResponseDto(Cart cart)
        {
            return new CartResponseDto
            {
                Id = cart.Id,
                Status = cart.Status ?? "Pending",
                UserId = cart.UserId,
                TotalPrice = cart.CartItems?.Sum(ci => ci.Quantity * (ci.Product?.Price ?? 0)) ?? 0,
                Items = cart.CartItems?.Select(ci => new CartItemResponseDto
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
                }).ToList() ?? new List<CartItemResponseDto>()
            };
        }

        private static Cart CreateCartFromDto(CartCreateDto dto)
        {
            return new Cart
            {
                UserId = dto.UserId,
                Status = "Active",
                CartItems = dto.Items.Select(i => new CartItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };
        }
    }
}