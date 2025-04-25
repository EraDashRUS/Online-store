using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Data;
using OnlineStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<CartItem> _cartItemRepository;

        public CartService(
            ApplicationDbContext context,
            IRepository<Cart> cartRepository,
            IRepository<CartItem> cartItemRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _cartItemRepository = cartItemRepository ?? throw new ArgumentNullException(nameof(cartItemRepository));
        }

        public async Task<CartResponseDto> GetCartByUserIdAsync(int userId)
        {
            // Сначала проверяем существование пользователя
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new NotFoundException($"Пользователь с ID {userId} не найден");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _cartRepository.AddAsync(cart);
                await _context.SaveChangesAsync(); // Сохраняем новую корзину
            }

            return MapToDto(cart);
        }

        public async Task<CartItemResponseDto> AddToCartAsync(CartItemCreateDto itemDto)
        {
            var cart = await _context.Carts
        .Include(c => c.User)
        .FirstOrDefaultAsync(c => c.Id == itemDto.CartId);

            if (cart == null)
                throw new NotFoundException("Корзина не найдена");

            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
                throw new NotFoundException("Товар не найден");

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(i => i.CartId == itemDto.CartId && i.ProductId == itemDto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += itemDto.Quantity;
                await _cartItemRepository.UpdateAsync(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = itemDto.CartId,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity
                };
                await _cartItemRepository.AddAsync(newItem);
                existingItem = newItem;
            }

            await _context.SaveChangesAsync();
            return ConvertItemToDto(existingItem);
        }

        public async Task<bool> RemoveFromCartAsync(int itemId)
        {
            var item = await _cartItemRepository.GetByIdAsync(itemId);
            if (item == null)
                return false;

            await _cartItemRepository.DeleteAsync(itemId);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return false;

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CartResponseDto> UpdateCartItemAsync(CartItemUpdateDto itemDto)
        {
            var item = await _cartItemRepository.GetByIdAsync(itemDto.Id);
            if (item == null)
                throw new ArgumentException("Элемент корзины не найден");

            // Ручное обновление полей
            item.Quantity = itemDto.Quantity;

            await _cartItemRepository.UpdateAsync(item);
            await _context.SaveChangesAsync();

            return await GetCartByUserIdAsync(
                (await _cartRepository.GetByIdAsync(item.CartId)).UserId);
        }

        public async Task<CartResponseDto> GetUserCartAsync(int userId, int cartId)
        {
            // Получаем корзину с элементами и связанными продуктами
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);

            if (cart == null)
            {
                throw new NotFoundException($"Корзина с ID {cartId} не найдена или не принадлежит пользователю");
            }

            return new CartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.CartItems.Select(ci => new CartItemResponseDto
                {
                    Id = ci.Id,
                    CartId = ci.CartId,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.Name ?? "Неизвестный товар",
                    ProductPrice = ci.Product?.Price ?? 0m,
                    Quantity = ci.Quantity,
                    IsAvailable = ci.Product?.StockQuantity > 0
                }).ToList(),
                TotalPrice = cart.CartItems.Sum(ci => ci.Quantity * (ci.Product?.Price ?? 0m))
            };
        }

        public async Task<CartResponseDto> CreateCartForUserAsync(int userId)
        {
            var cart = new Cart { UserId = userId };
            await _cartRepository.AddAsync(cart);
            await _context.SaveChangesAsync();
            return MapToDto(cart);
        }

        public async Task<List<CartResponseDto>> GetUserCartsAsync(int userId)
        {
            return await _context.Carts
                .Where(c => c.UserId == userId)
                .Select(c => new CartResponseDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    TotalPrice = c.CartItems.Sum(ci => ci.Quantity * (ci.Product.Price))
                })
                .ToListAsync();
        }

        private CartResponseDto MapToDto(Cart cart)
        {
            return new CartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.CartItems.Select(ConvertItemToDto).ToList(),
                TotalPrice = cart.CartItems.Sum(i => i.Quantity * i.Product.Price)
            };
        }

        private CartItemResponseDto ConvertItemToDto(CartItem item)
        {
            return new CartItemResponseDto
            {
                Id = item.Id,
                CartId = item.CartId,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "Неизвестный товар",
                ProductPrice = item.Product?.Price ?? 0m,
                Quantity = item.Quantity,
                IsAvailable = item.Product?.StockQuantity > 0 // Проверка наличия на складе
            };
        }

    }
}