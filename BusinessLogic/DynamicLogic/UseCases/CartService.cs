using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    /// <summary>
    /// Сервис для работы с корзиной покупок
    /// </summary>
    /// <remarks>
    /// Инициализирует новый экземпляр сервиса корзины
    /// </remarks>
    /// <param name="context">Контекст базы данных</param>
    /// <param name="cartRepository">Репозиторий корзины</param>
    /// <param name="cartItemRepository">Репозиторий элементов корзины</param>
    public class CartService(
        ApplicationDbContext context,
        IRepository<Cart> cartRepository,
        IRepository<CartItem> cartItemRepository) : ICartService
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IRepository<Cart> _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        private readonly IRepository<CartItem> _cartItemRepository = cartItemRepository ?? throw new ArgumentNullException(nameof(cartItemRepository));

        /// <summary>
        /// Получает корзину пользователя по его идентификатору
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные корзины</returns>
        /// <exception cref="NotFoundException">Пользователь не найден</exception>
        public async Task<CartResponseDto> GetCartByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
            if (!userExists)
            {
                throw new NotFoundException($"Пользователь с ID {userId} не найден");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _cartRepository.AddAsync(cart, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return MapToDto(cart);
        }

        /// <summary>
        /// Добавляет товар в корзину
        /// </summary>
        /// <param name="itemDto">Данные добавляемого товара</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные добавленного товара</returns>
        /// <exception cref="NotFoundException">Корзина или товар не найдены</exception>
        public async Task<CartItemResponseDto> AddToCartAsync(CartItemCreateDto itemDto, CancellationToken cancellationToken = default)
        {
            var cart = await _context.Carts
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == itemDto.CartId, cancellationToken) ?? throw new NotFoundException("Корзина не найдена");
            var product = await _context.Products.FindAsync(new object[] { itemDto.ProductId }, cancellationToken) ?? throw new NotFoundException("Товар не найден");
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(i => i.CartId == itemDto.CartId && i.ProductId == itemDto.ProductId, cancellationToken);

            if (existingItem != null)
            {
                existingItem.Quantity += itemDto.Quantity;
                await _cartItemRepository.UpdateAsync(existingItem, cancellationToken);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = itemDto.CartId,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity
                };
                await _cartItemRepository.AddAsync(newItem, cancellationToken);
                existingItem = newItem;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return ConvertItemToDto(existingItem);
        }

        /// <summary>
        /// Удаляет товар из корзины
        /// </summary>
        /// <param name="itemId">Идентификатор элемента корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True если элемент успешно удален, иначе false</returns>
        public async Task<bool> RemoveFromCartAsync(int itemId, CancellationToken cancellationToken = default)
        {
            var item = await _cartItemRepository.GetByIdAsync(itemId, cancellationToken);
            if (item == null)
                return false;

            await _cartItemRepository.DeleteAsync(itemId);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Очищает корзину пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True если корзина успешно очищена, иначе false</returns>
        public async Task<bool> ClearCartAsync(int userId, CancellationToken cancellationToken = default)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (cart == null || cart.CartItems.Count == 0)
                return false;

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Обновляет элемент корзины
        /// </summary>
        /// <param name="itemDto">Данные обновления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленные данные корзины</returns>
        /// <exception cref="ArgumentException">Элемент корзины не найден</exception>
        public async Task<CartResponseDto> UpdateCartItemAsync(CartItemUpdateDto itemDto, CancellationToken cancellationToken = default)
        {
            var item = await _cartItemRepository.GetByIdAsync(itemDto.Id) ?? throw new ArgumentException("Элемент корзины не найден");

            // Ручное обновление полей
            item.Quantity = itemDto.Quantity;

            await _cartItemRepository.UpdateAsync(item);
            await _context.SaveChangesAsync(cancellationToken);

            return await GetCartByUserIdAsync(
                (await _cartRepository.GetByIdAsync(item.CartId)).UserId, cancellationToken);
        }

        /// <summary>
        /// Получает корзину пользователя по идентификаторам пользователя и корзины
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные корзины</returns>
        /// <exception cref="NotFoundException">Корзина не найдена или не принадлежит пользователю</exception>
        public async Task<CartResponseDto> GetUserCartAsync(int userId, int cartId, CancellationToken cancellationToken = default)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId, cancellationToken);

            return cart == null
                ? throw new NotFoundException($"Корзина с ID {cartId} не найдена или не принадлежит пользователю")
                : new CartResponseDto
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

        /// <summary>
        /// Создает новую корзину для пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные созданной корзины</returns>
        public async Task<CartResponseDto> CreateCartForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var cart = new Cart { UserId = userId };
            await _cartRepository.AddAsync(cart, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return MapToDto(cart);
        }

        /// <summary>
        /// Получает список всех корзин пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список корзин пользователя</returns>
        public async Task<List<CartResponseDto>> GetUserCartsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .Where(c => c.UserId == userId)
                .Select(c => new CartResponseDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    TotalPrice = c.CartItems.Sum(ci => ci.Quantity * (ci.Product.Price))
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Преобразует корзину в DTO
        /// </summary>
        /// <param name="cart">Корзина</param>
        /// <returns>DTO корзины</returns>
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

        /// <summary>
        /// Преобразует элемент корзины в DTO
        /// </summary>
        /// <param name="item">Элемент корзины</param>
        /// <returns>DTO элемента корзины</returns>
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
                IsAvailable = item.Product?.StockQuantity > 0
            };
        }

    }
}