using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления корзинами покупок
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера корзин
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public CartsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает список всех корзин с пользователями и товарами
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Список корзин</returns>
        /// <response code="200">Успешно возвращен список корзин</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Cart>>> GetAllCarts(CancellationToken cancellationToken)
        {
            return await _context.Carts
                .Include(c => c.User)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
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
        public async Task<ActionResult<Cart>> GetCart(int id, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (cart == null) return NotFound();
            return cart;
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
        public async Task<ActionResult<Cart>> PostCart(Cart cart, CancellationToken cancellationToken)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction("GetCart", new { id = cart.Id }, cart);
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