using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Контроллер для работы с элементами корзины покупок
    /// </summary>
    /// <remarks>
    /// Инициализирует новый экземпляр <see cref="CartItemsController"/>
    /// </remarks>
    /// <param name="context">Контекст базы данных</param>
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemsController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Получает все элементы корзины с информацией о товарах и корзинах
        /// </summary>
        /// <returns>Список элементов корзины</returns>
        /// <response code="200">Возвращает список элементов корзины</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems(CancellationToken cancellationToken)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Получает конкретный элемент корзины по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор элемента корзины</param>
        /// <returns>Элемент корзины</returns>
        /// <response code="200">Элемент корзины найден</response>
        /// <response code="404">Элемент корзины не найден</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartItem>> GetCartItem(int id, CancellationToken cancellationToken)
        {
            var cartItem = await _context.CartItems.FindAsync(new object[] { id }, cancellationToken);

            if (cartItem == null)
            {
                return NotFound();
            }

            return cartItem;
        }

        /// <summary>
        /// Обновляет существующий элемент корзины
        /// </summary>
        /// <param name="id">Идентификатор элемента</param>
        /// <param name="cartItem">Данные для обновления</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Элемент успешно обновлен</response>
        /// <response code="400">Неверный идентификатор</response>
        /// <response code="404">Элемент не найден</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutCartItem(int id, CartItem cartItem, CancellationToken cancellationToken)
        {
            if (id != cartItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(cartItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartItemExists(id))
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
        /// Добавляет новый товар в корзину
        /// </summary>
        /// <param name="dto">DTO с данными для добавления товара</param>
        /// <returns>Созданный элемент корзины</returns>
        /// <response code="201">Товар успешно добавлен</response>
        /// <response code="400">Недостаточно товара на складе</response>
        /// <response code="404">Корзина или товар не найдены</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartItem>> AddToCart([FromBody] CartItemCreateDto dto, CancellationToken cancellationToken)
        {
            var (cart, product) = await ValidateRequestAsync(dto, cancellationToken);
            if (cart == null || product == null)
                return NotFound("Корзина или товар не найдены");

            if (!CheckStock(product, dto.Quantity))
                return BadRequest("Недостаточно товара на складе");

            var cartItem = await CreateCartItemAsync(dto, product, cancellationToken);
            return CreatedAtAction(nameof(GetCartItem), new { id = cartItem.Id }, MapToResponse(cartItem));
        }

        /// <summary>
        /// Проверяет существование корзины и товара
        /// </summary>
        /// <param name="dto">DTO с данными для проверки</param>
        /// <returns>Кортеж с корзиной и товаром</returns>
        private async Task<(Cart? cart, Product? product)> ValidateRequestAsync(CartItemCreateDto dto, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FindAsync(new object[] { dto.CartId }, cancellationToken);
            var product = await _context.Products.FindAsync(new object[] { dto.ProductId }, cancellationToken);
            return (cart, product);
        }

        /// <summary>
        /// Проверяет наличие достаточного количества товара на складе
        /// </summary>
        /// <param name="product">Товар</param>
        /// <param name="quantity">Запрашиваемое количество</param>
        /// <returns>True если товара достаточно, иначе False</returns>
        private static bool CheckStock(Product product, int quantity)
        {
            return product.StockQuantity >= quantity;
        }

        /// <summary>
        /// Создает новый элемент корзины
        /// </summary>
        /// <param name="dto">DTO с данными элемента</param>
        /// <param name="product">Товар</param>
        /// <returns>Созданный элемент корзины</returns>
        private async Task<CartItem> CreateCartItemAsync(CartItemCreateDto dto, Product product, CancellationToken cancellationToken)
        {
            var cartItem = new CartItem
            {
                CartId = dto.CartId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };

            product.StockQuantity -= dto.Quantity;
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync(cancellationToken);
            return cartItem;
        }

        /// <summary>
        /// Преобразует элемент корзины в формат ответа
        /// </summary>
        /// <param name="item">Элемент корзины</param>
        /// <returns>Объект ответа</returns>
        private static object MapToResponse(CartItem item)
        {
            return new { item.Id, item.CartId, item.ProductId, item.Quantity };
        }

        /// <summary>
        /// Удаляет элемент из корзины
        /// </summary>
        /// <param name="id">Идентификатор элемента</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Элемент успешно удален</response>
        /// <response code="404">Элемент не найден</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCartItem(int id, CancellationToken cancellationToken)
        {
            var cartItem = await _context.CartItems.FindAsync(new object[] { id }, cancellationToken);
            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Проверяет существование элемента корзины
        /// </summary>
        /// <param name="id">Идентификатор элемента</param>
        /// <returns>True если элемент существует, иначе False</returns>
        private bool CartItemExists(int id)
        {
            return _context.CartItems.Any(e => e.Id == id);
        }
    }
}