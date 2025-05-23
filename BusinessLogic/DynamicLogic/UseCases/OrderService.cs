using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Models;
using OnlineStore.Storage.Data;
using OnlineStore.BusinessLogic.DynamicLogic.UseCases;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    /// <summary>
    /// Сервис для работы с заказами
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        private readonly IAdminCommentService _adminCommentService;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса заказов
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public OrderService(ApplicationDbContext context, IAdminCommentService adminCommentService)
        {
            _context = context;
            _adminCommentService = adminCommentService;
        }

        /// <summary>
        /// Создает заказ на основе корзины
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о созданном заказе</returns>
        /// <exception cref="NotFoundException">Корзина не найдена</exception>
        /// <exception cref="InvalidOperationException">Корзина пуста</exception>
        public async Task<OrderDto> CreateOrderFromCartAsync(int cartId, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

            if (cart == null) throw new NotFoundException("Корзина не найдена");
            if (!cart.CartItems.Any()) throw new InvalidOperationException("Корзина пуста");

            cart.Status = "Pending";
            await _context.SaveChangesAsync(cancellationToken);

            return new OrderDto
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                Status = cart.Status,
                Items = cart.CartItems.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product.Price
                }).ToList(),
                TotalAmount = cart.CartItems.Sum(i => i.Quantity * i.Product.Price)
            };
        }

        /// <summary>
        /// Получает информацию о заказе
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Информация о заказе</returns>
        /// <exception cref="NotFoundException">Заказ не найден</exception>
        public async Task<OrderDto> GetOrderAsync(int cartId, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FindAsync(new object?[] { cartId }, cancellationToken: cancellationToken);

            if (cart == null) throw new NotFoundException("Cart not found");

            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .Include(ci => ci.Product)
                .ToListAsync(cancellationToken);

            return new OrderDto
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                Status = cart.Status ?? "Pending",
                Items = cartItems.Select(ci => new OrderItemDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product.Price
                }).ToList(),
                TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price)
            };
        }

        /// <summary>
        /// Получает список всех заказов
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список заказов</returns>
        public async Task<List<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken)
        {
            var orderCarts = await _context.Carts
                .Where(c => c.Status != null)
                .Include(c => c.User)
                .ToListAsync(cancellationToken);

            var result = new List<OrderDto>();

            foreach (var cart in orderCarts)
            {
                var cartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.Id)
                    .Include(ci => ci.Product)
                    .ToListAsync(cancellationToken);

                result.Add(new OrderDto
                {
                    CartId = cart.Id,
                    UserId = cart.UserId,
                    Status = cart.Status ?? "Pending",
                    Items = cartItems.Select(ci => new OrderItemDto
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.Product.Price
                    }).ToList(),
                    TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price)
                });
            }

            return result;
        }

        /// <summary>
        /// Обновляет статус заказа
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="statusDto">Новый статус</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленная информация о заказе</returns>
        /// <exception cref="NotFoundException">Заказ не найден</exception>
        public async Task<OrderDto> UpdateOrderStatusAsync(
    int cartId,
    OrderStatusUpdateDto statusDto,
    CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FindAsync(new object[] { cartId }, cancellationToken);
            if (cart == null) throw new NotFoundException("Order not found");

            cart.Status = statusDto.NewStatus;

            // Сохраняем комментарий, если он есть
            if (!string.IsNullOrEmpty(statusDto.AdminComment))
            {
                _adminCommentService.AddComment(cartId, statusDto.AdminComment);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return await GetOrderAsync(cartId, cancellationToken);
        }

        /// <summary>
        /// Обновляет информацию о заказе
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="orderDto">Обновленная информация</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленная информация о заказе</returns>
        /// <exception cref="NotFoundException">Заказ не найден</exception>
        public async Task<OrderDto> UpdateOrderAsync(
            int cartId,
            OrderDto orderDto,
            CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken) ?? throw new NotFoundException("Order not found");
            var currentItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync(cancellationToken);

            cart.Status = orderDto.Status;
            await _context.SaveChangesAsync(cancellationToken);

            return await GetOrderAsync(cartId, cancellationToken);
        }

        /// <summary>
        /// Получает заказ с комментарием администратора
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Заказ с комментарием</returns>
        public async Task<OrderWithCommentDto> GetOrderWithCommentAsync(int cartId, CancellationToken cancellationToken)
        {
            var order = await GetOrderAsync(cartId, cancellationToken);
            var comment = _adminCommentService.GetComment(cartId);

            return new OrderWithCommentDto
            {
                CartId = order.CartId,
                UserId = order.UserId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Items = order.Items,
                AdminComment = comment
            };
        }

        public async Task<AdminOrderDto> GetAdminOrderAsync(int cartId, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

            if (cart == null) throw new NotFoundException("Cart not found");

            // Гарантируем, что статус не будет null
            var status = string.IsNullOrEmpty(cart.Status) ? "Undefined" : cart.Status;

            return new AdminOrderDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Status = status,
                UserEmail = cart.User?.Email ?? "unknown",
                Items = cart.CartItems.Select(ci => new OrderItemDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.Name ?? "Unknown",
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product?.Price ?? 0
                }).ToList(),
                TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * (ci.Product?.Price ?? 0))
            };
        }




    }
}