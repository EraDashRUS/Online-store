using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CartItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems()
        {
            return await _context.CartItems
                .Include(ci => ci.Product)  // Подгружаем товар
                .Include(ci => ci.Cart)     // Подгружаем корзину
                .ToListAsync();
        }

        // GET: api/CartItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CartItem>> GetCartItem(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);

            if (cartItem == null)
            {
                return NotFound();
            }

            return cartItem;
        }

        // PUT: api/CartItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCartItem(int id, CartItem cartItem)
        {
            if (id != cartItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(cartItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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

        // POST: api/CartItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CartItem>> AddToCart([FromBody] CartItemCreateDto dto)
        {
            var (cart, product) = await ValidateRequestAsync(dto);
            if (cart == null || product == null)
                return NotFound("Корзина или товар не найдены");

            if (!CheckStock(product, dto.Quantity))
                return BadRequest("Недостаточно товара на складе");

            var cartItem = await CreateCartItemAsync(dto, product);
            return CreatedAtAction(nameof(GetCartItem), new { id = cartItem.Id }, MapToResponse(cartItem));
        }

        // Валидация (17 строк)
        private async Task<(Cart? cart, Product? product)> ValidateRequestAsync(CartItemCreateDto dto)
        {
            var cart = await _context.Carts.FindAsync(dto.CartId);
            var product = await _context.Products.FindAsync(dto.ProductId);
            return (cart, product);
        }

        // Проверка остатков (3 строки)
        private bool CheckStock(Product product, int quantity)
        {
            return product.StockQuantity >= quantity;
        }

        // Создание записи (12 строк)
        private async Task<CartItem> CreateCartItemAsync(CartItemCreateDto dto, Product product)
        {
            var cartItem = new CartItem
            {
                CartId = dto.CartId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };

            product.StockQuantity -= dto.Quantity;
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        // Маппинг для ответа (5 строк)
        private object MapToResponse(CartItem item)
        {
            return new { item.Id, item.CartId, item.ProductId, item.Quantity };
        }

        // DELETE: api/CartItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartItemExists(int id)
        {
            return _context.CartItems.Any(e => e.Id == id);
        }
    }
}
