using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;
using System.Security.Claims;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления корзинами покупок
    /// </summary>
    /// <remarks>
    /// Инициализирует новый экземпляр контроллера корзин
    /// </remarks>
    /// <param name="context">Контекст базы данных</param>
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Получает список всех корзин с пользователями и товарами
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Список корзин</returns>
        /// <response code="200">Успешно возвращен список корзин</response>
        [HttpGet]
        public async Task<ActionResult<List<CartResponseDto>>> GetAllCarts(CancellationToken cancellationToken)
        {
            var carts = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .Select(c => new CartResponseDto
                {
                    Id = c.Id,
                    Status = c.Status ?? "Pending",
                    UserId = c.UserId,
                    TotalPrice = c.CartItems.Sum(ci => ci.Quantity * ci.Product.Price),
                    Items = c.CartItems.Select(ci => new CartItemResponseDto
                    {
                        Id = ci.Id,
                        Quantity = ci.Quantity,
                        ProductId = ci.ProductId,
                        Product = new ProductBriefDto
                        {
                            Id = ci.Product.Id,
                            Name = ci.Product.Name,
                            Description = ci.Product.Description,
                            Price = ci.Product.Price,
                            StockQuantity = ci.Product.StockQuantity
                        }
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            return Ok(carts);
        }

        /// <summary>
        /// Получает корзину по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор корзины</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Данные корзины</returns>
        /// <response code="200">Корзина найдена</response>
        /// <response code="404">Корзина не найдена</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartResponseDto>> GetCart(int id, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product) // Добавляем загрузку продукта
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (cart == null) return NotFound();

            var result = new CartResponseDto
            {
                Id = cart.Id,
                Status = cart.Status ?? "Pending", // Обработка NULL
                UserId = cart.UserId,
                Items = cart.CartItems.Select(ci => new CartItemResponseDto
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
                }).ToList(),
                TotalPrice = cart.CartItems.Sum(ci => ci.Quantity * (ci.Product?.Price ?? 0))
            };

            return Ok(result);
        }

        /// <summary>
        /// Обновляет данные корзины
        /// </summary>
        /// <param name="id">Идентификатор корзины</param>
        /// <param name="cart">Обновленные данные корзины</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Корзина успешно обновлена</response>
        /// <response code="400">Неверный идентификатор</response>
        /// <response code="404">Корзина не найдена</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutCart(int id, Cart cart, CancellationToken cancellationToken)
        {
            if (id != cart.Id)
            {
                return BadRequest();
            }

            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Создает новую корзину
        /// </summary>
        /// <param name="cart">Данные новой корзины</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Созданная корзина</returns>
        /// <response code="201">Корзина успешно создана</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CartResponseDto>> CreateCart(
    [FromBody] CartCreateDto dto,
    CancellationToken cancellationToken)
        {
            // 1. Проверяем, что пользователь существует
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }

            // 2. Создаём корзину
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var cart = new Cart
                {
                    UserId = dto.UserId,  // Берём ID из DTO
                    Status = "Active",
                    CartItems = dto.Items.Select(i => new CartItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                };

                await _context.Carts.AddAsync(cart, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // 3. Загружаем данные для ответа
                var createdCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == cart.Id, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                // 4. Возвращаем ответ
                return CreatedAtAction(
                    nameof(GetCart),
                    new { id = cart.Id },
                    ConvertToCartResponseDto(createdCart));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return StatusCode(500, $"Ошибка при создании корзины: {ex.Message}");
            }
        }

        // Вспомогательный метод для маппинга
        private CartResponseDto ConvertToCartResponseDto(Cart cart)
        {
            return new CartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Status = cart.Status
            };
        }

        /// <summary>
        /// Удаляет корзину
        /// </summary>
        /// <param name="id">Идентификатор корзины</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Корзина успешно удалена</response>
        /// <response code="404">Корзина не найдена</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCart(int id, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FindAsync(new object[] { id }, cancellationToken);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Проверяет существование корзины
        /// </summary>
        /// <param name="id">Идентификатор корзины</param>
        /// <returns>True если корзина существует, иначе False</returns>
        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }
    }
}