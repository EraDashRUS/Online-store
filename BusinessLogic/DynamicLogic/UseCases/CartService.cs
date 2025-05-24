using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Product;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.CartItem;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    /// <summary>
    /// Сервис для управления корзинами покупок пользователей.
    /// Предоставляет методы для добавления, удаления, обновления и получения корзин и их содержимого.
    /// </summary>
    public class CartService(
        ApplicationDbContext context,
        IRepository<Cart> cartRepository,
        IRepository<CartItem> cartItemRepository) : ICartService
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IRepository<Cart> _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        private readonly IRepository<CartItem> _cartItemRepository = cartItemRepository ?? throw new ArgumentNullException(nameof(cartItemRepository));

        /// <summary>
        /// Получить корзину пользователя по его идентификатору.
        /// Если корзина отсутствует, создаёт новую.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO корзины</returns>
        /// <exception cref="NotFoundException">Если пользователь не найден</exception>
        public async Task<CartResponseDto> GetCartByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            if (!await UserExistsAsync(userId, cancellationToken))
                throw new NotFoundException($"Пользователь с ID {userId} не найден");

            var cart = await GetCartWithItemsByUserIdAsync(userId, cancellationToken);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _cartRepository.AddAsync(cart, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return MapToDto(cart);
        }

        /// <summary>
        /// Проверяет существование пользователя по идентификатору.
        /// </summary>
        private async Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        }

        /// <summary>
        /// Получает корзину пользователя с элементами и товарами по идентификатору пользователя.
        /// </summary>
        private async Task<Cart?> GetCartWithItemsByUserIdAsync(int userId, CancellationToken cancellationToken)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        }

        /// <summary>
        /// Добавляет товар в корзину.
        /// Если товар уже есть в корзине, увеличивает его количество.
        /// </summary>
        /// <param name="itemDto">Данные для добавления товара в корзину</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO элемента корзины</returns>
        /// <exception cref="NotFoundException">Если корзина или товар не найдены</exception>
        public async Task<CartItemResponseDto> AddToCartAsync(CartItemCreateDto itemDto, CancellationToken cancellationToken = default)
        {
            var cart = await _context.Carts
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == itemDto.CartId, cancellationToken)
                ?? throw new NotFoundException("Корзина не найдена");

            var product = await _context.Products.FindAsync(new object[] { itemDto.ProductId }, cancellationToken)
                ?? throw new NotFoundException("Товар не найден");

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(i => i.CartId == itemDto.CartId && i.ProductId == itemDto.ProductId, cancellationToken);

            if (existingItem != null)
            {
                existingItem.Quantity += itemDto.Quantity;
                await _cartItemRepository.UpdateAsync(existingItem, cancellationToken);
            }
            else
            {
                existingItem = await AddNewCartItemAsync(itemDto, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return ConvertItemToDto(existingItem);
        }

        /// <summary>
        /// Добавляет новый элемент в корзину.
        /// </summary>
        private async Task<CartItem> AddNewCartItemAsync(CartItemCreateDto itemDto, CancellationToken cancellationToken)
        {
            var newItem = new CartItem
            {
                CartId = itemDto.CartId,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity
            };
            await _cartItemRepository.AddAsync(newItem, cancellationToken);
            return newItem;
        }

        /// <summary>
        /// Удаляет элемент из корзины по его идентификатору.
        /// </summary>
        /// <param name="itemId">Идентификатор элемента корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True, если элемент был удалён, иначе false</returns>
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
        /// Очищает корзину пользователя (удаляет все элементы).
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True, если корзина была очищена, иначе false</returns>
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
        /// Обновляет количество товара в элементе корзины.
        /// </summary>
        /// <param name="itemDto">Данные для обновления элемента корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO корзины</returns>
        /// <exception cref="ArgumentException">Если элемент корзины не найден</exception>
        public async Task<CartResponseDto> UpdateCartItemAsync(CartItemUpdateDto itemDto, CancellationToken cancellationToken = default)
        {
            var item = await _cartItemRepository.GetByIdAsync(itemDto.Id)
                ?? throw new ArgumentException("Элемент корзины не найден");

            item.Quantity = itemDto.Quantity;

            await _cartItemRepository.UpdateAsync(item);
            await _context.SaveChangesAsync(cancellationToken);

            var cart = await _cartRepository.GetByIdAsync(item.CartId);
            return await GetCartByUserIdAsync(cart.UserId, cancellationToken);
        }

        /// <summary>
        /// Получает корзину пользователя по идентификатору пользователя и корзины.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO корзины</returns>
        /// <exception cref="NotFoundException">Если корзина не найдена или не принадлежит пользователю</exception>
        public async Task<CartResponseDto> GetUserCartAsync(int userId, int cartId, CancellationToken cancellationToken = default)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId, cancellationToken);

            if (cart == null)
                throw new NotFoundException($"Корзина с ID {cartId} не найдена или не принадлежит пользователю");

            return MapToDto(cart);
        }

        /// <summary>
        /// Создаёт новую корзину для пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO корзины</returns>
        public async Task<CartResponseDto> CreateCartForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var cart = new Cart { UserId = userId };
            await _cartRepository.AddAsync(cart, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return MapToDto(cart);
        }

        /// <summary>
        /// Получает список всех корзин пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список DTO корзин</returns>
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
        /// Преобразует сущность корзины в DTO.
        /// </summary>
        private CartResponseDto MapToDto(Cart cart)
        {
            return new CartResponseDto
            {
                Id = cart.Id,
                Status = cart.Status ?? "Pending",
                UserId = cart.UserId,
                Items = cart.CartItems.Select(ConvertItemToDto).ToList(),
                TotalPrice = cart.CartItems.Sum(i => i.Quantity * (i.Product?.Price ?? 0))
            };
        }

        /// <summary>
        /// Преобразует элемент корзины в DTO.
        /// </summary>
        private CartItemResponseDto ConvertItemToDto(CartItem item)
        {
            return new CartItemResponseDto
            {
                Id = item.Id,
                Quantity = item.Quantity,
                ProductId = item.ProductId,
                Product = item.Product != null ? new ProductBriefDto
                {
                    Id = item.Product.Id,
                    Name = item.Product.Name,
                    Price = item.Product.Price,
                    StockQuantity = item.Product.StockQuantity
                } : null
            };
        }
    }
}