using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.Storage.Data;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Admin;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Order;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    /// <summary>
    /// Сервис для управления заказами: создание, получение, обновление, получение с комментарием администратора.
    /// </summary>
    /// <remarks>
    /// Конструктор сервиса заказов.
    /// </remarks>
    public class OrderService(ApplicationDbContext context, IAdminCommentService adminCommentService) : IOrderService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IAdminCommentService _adminCommentService = adminCommentService;

        /// <summary>
        /// Создать заказ на основе корзины.
        /// </summary>
        /// <exception cref="NotFoundException">Если корзина не найдена</exception>
        /// <exception cref="InvalidOperationException">Если корзина пуста</exception>
        public async Task<OrderDto> CreateOrderFromCartAsync(int cartId, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

            if (cart == null)
                throw new NotFoundException("Корзина не найдена");
            if (!cart.CartItems.Any())
                throw new InvalidOperationException("Корзина пуста");

            cart.Status = "Pending";
            await _context.SaveChangesAsync(cancellationToken);

            return MapCartToOrderDto(cart);
        }

        /// <summary>
        /// Получить заказ по идентификатору корзины.
        /// </summary>
        /// <exception cref="NotFoundException">Если заказ не найден</exception>
        public async Task<OrderDto> GetOrderAsync(int cartId, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FindAsync(new object?[] { cartId }, cancellationToken: cancellationToken);
            if (cart == null)
                throw new NotFoundException("Заказ не найден");

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
        /// Получить список всех заказов.
        /// </summary>
        public async Task<List<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken)
        {
            var orderCarts = await _context.Carts
                .Where(c => c.Status != null)
                .Include(c => c.User)
                .ToListAsync(cancellationToken);

            var result = new List<OrderDto>();
            foreach (var cart in orderCarts)
            {
                var orderDto = await BuildOrderDtoForCart(cart, cancellationToken);
                result.Add(orderDto);
            }
            return result;
        }

        private async Task<OrderDto> BuildOrderDtoForCart(Storage.Models.Cart cart, CancellationToken cancellationToken)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
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
        /// Обновить статус заказа.
        /// </summary>
        /// <exception cref="NotFoundException">Если заказ не найден</exception>
        public async Task<OrderDto> UpdateOrderStatusAsync(
            int cartId,
            OrderStatusUpdateDto statusDto,
            CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FindAsync(new object[] { cartId }, cancellationToken);
            if (cart == null)
                throw new NotFoundException("Заказ не найден");

            cart.Status = statusDto.NewStatus;

            if (!string.IsNullOrEmpty(statusDto.AdminComment))
                _adminCommentService.AddComment(cartId, statusDto.AdminComment);

            await _context.SaveChangesAsync(cancellationToken);

            return await GetOrderAsync(cartId, cancellationToken);
        }

        /// <summary>
        /// Обновить заказ.
        /// </summary>
        /// <exception cref="NotFoundException">Если заказ не найден</exception>
        public async Task<OrderDto> UpdateOrderAsync(
            int cartId,
            OrderDto orderDto,
            CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken)
                ?? throw new NotFoundException("Заказ не найден");

            cart.Status = orderDto.Status;
            await _context.SaveChangesAsync(cancellationToken);

            return await GetOrderAsync(cartId, cancellationToken);
        }

        /// <summary>
        /// Получить заказ с комментарием администратора.
        /// </summary>
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

        /// <summary>
        /// Получить заказ для администратора.
        /// </summary>
        public async Task<AdminOrderDto> GetAdminOrderAsync(int cartId, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

            if (cart == null)
                throw new NotFoundException("Заказ не найден");

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

        private static OrderDto MapCartToOrderDto(Storage.Models.Cart cart)
        {
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
    }
}