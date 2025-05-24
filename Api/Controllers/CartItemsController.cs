using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Product;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.CartItem;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;

namespace OnlineStore.Api.Controllers
{
    /// <summary>
    /// Контроллер для управления элементами корзины покупателя.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemsController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Получить элемент корзины по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор элемента корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные элемента корзины</returns>
        /// <response code="200">Элемент найден</response>
        /// <response code="404">Элемент не найден</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartItemResponseDto>> GetCartItem(int id, CancellationToken cancellationToken)
        {
            var cartItem = await GetCartItemWithDetails(id, cancellationToken);
            if (cartItem == null)
                return NotFound();
            return Ok(cartItem);
        }

        private async Task<CartItemResponseDto?> GetCartItemWithDetails(int id, CancellationToken cancellationToken)
        {
            return await _context.CartItems
                .AsNoTracking()
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                    .ThenInclude(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                .Where(ci => ci.Id == id)
                .Select(ci => new CartItemResponseDto
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
                    },
                    Cart = new CartBriefDto
                    {
                        Id = ci.Cart.Id,
                        Status = ci.Cart.Status ?? "Pending",
                        ItemsCount = ci.Cart.CartItems.Sum(item => item.Quantity),
                        TotalPrice = ci.Cart.CartItems.Sum(item => item.Quantity * item.Product.Price)
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Получить список всех элементов корзины (только для администратора).
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список элементов корзины</returns>
        /// <response code="200">Список элементов корзины</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CartItemResponseDto>>> GetCartItems(CancellationToken cancellationToken)
        {
            var cartItems = await GetAllCartItemsWithDetails(cancellationToken);
            return Ok(cartItems);
        }

        private async Task<List<CartItemResponseDto>> GetAllCartItemsWithDetails(CancellationToken cancellationToken)
        {
            return await _context.CartItems
                .AsNoTracking()
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                    .ThenInclude(c => c.CartItems)
                .Select(ci => new CartItemResponseDto
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
                    },
                    Cart = new CartBriefDto
                    {
                        Id = ci.Cart.Id,
                        Status = ci.Cart.Status ?? "Pending",
                        ItemsCount = ci.Cart.CartItems.Sum(item => item.Quantity),
                        TotalPrice = ci.Cart.CartItems.Sum(item => item.Quantity * item.Product.Price)
                    }
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Обновить существующий элемент корзины.
        /// </summary>
        /// <param name="id">Идентификатор элемента корзины</param>
        /// <param name="cartItem">Данные для обновления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Элемент обновлен</response>
        /// <response code="400">Неверный идентификатор</response>
        /// <response code="404">Элемент не найден</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutCartItem(int id, CartItem cartItem, CancellationToken cancellationToken)
        {
            if (id != cartItem.Id)
                return BadRequest();

            _context.Entry(cartItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartItemExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Добавить новый товар в корзину.
        /// </summary>
        /// <param name="dto">Данные для добавления товара</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Созданный элемент корзины</returns>
        /// <response code="201">Товар добавлен</response>
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

        private async Task<(Cart? cart, Product? product)> ValidateRequestAsync(CartItemCreateDto dto, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FindAsync(new object[] { dto.CartId }, cancellationToken);
            var product = await _context.Products.FindAsync(new object[] { dto.ProductId }, cancellationToken);
            return (cart, product);
        }

        private static bool CheckStock(Product product, int quantity)
        {
            return product.StockQuantity >= quantity;
        }

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

        private static object MapToResponse(CartItem item)
        {
            return new { item.Id, item.CartId, item.ProductId, item.Quantity };
        }

        /// <summary>
        /// Удалить элемент из корзины.
        /// </summary>
        /// <param name="id">Идентификатор элемента корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Элемент удален</response>
        /// <response code="404">Элемент не найден</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCartItem(int id, CancellationToken cancellationToken)
        {
            var cartItem = await _context.CartItems.FindAsync(new object[] { id }, cancellationToken);
            if (cartItem == null)
                return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        private bool CartItemExists(int id)
        {
            return _context.CartItems.Any(e => e.Id == id);
        }
    }
}