using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderDto> CreateOrderFromCartAsync(int cartId, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .Include(c => c.User)
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

            if (cart == null) throw new NotFoundException("Корзина не найдена");
            if (!cart.Items.Any()) throw new InvalidOperationException("Корзина пуста");

            // Создаем заказ на основе корзины
            cart.Status = "Pending"; // Новый статус
            await _context.SaveChangesAsync(cancellationToken);

            return new OrderDto
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                Status = cart.Status,
                Items = cart.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product.Price
                }).ToList(),
                TotalAmount = cart.Items.Sum(i => i.Quantity * i.Product.Price)
            };
        }

        public async Task<OrderDto> GetOrderAsync(int cartId, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FindAsync(cartId);

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

        public async Task<List<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken)
        {
            var orderCarts = await _context.Carts
                .Where(c => c.Status != null) // Корзины со статусом считаем заказами
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

        public async Task<OrderDto> UpdateOrderStatusAsync(
            int cartId,
            OrderStatusUpdateDto statusDto,
            CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FindAsync(new object[] { cartId }, cancellationToken);
            if (cart == null) throw new NotFoundException("Order not found");

            cart.Status = statusDto.NewStatus;
            await _context.SaveChangesAsync(cancellationToken);

            return await GetOrderAsync(cartId, cancellationToken);
        }

        public async Task<OrderDto> UpdateOrderAsync(
    int cartId,
    OrderDto orderDto,
    CancellationToken cancellationToken)
        {
            // Получаем корзину без элементов
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

            if (cart == null)
                throw new NotFoundException("Order not found");

            // Получаем текущие элементы корзины
            var currentItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync(cancellationToken);

            // В реальном проекте изменение состава заказа после создания
            // обычно не допускается. Здесь только обновление статуса.
            cart.Status = orderDto.Status;
            await _context.SaveChangesAsync(cancellationToken);

            return await GetOrderAsync(cartId, cancellationToken);
        }

        private async Task<OrderDto> MapCartToOrderDto(Cart cart)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .Include(ci => ci.Product)
                .ToListAsync();

            return new OrderDto
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                Status = cart.Status ?? "Pending",
                TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price),
                Items = cartItems.Select(ci => new OrderItemDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product.Price
                }).ToList()
            };
        }

        public async Task<OrderWithCommentDto> GetOrderWithCommentAsync(int cartId, CancellationToken cancellationToken)
        {
            var order = await GetOrderAsync(cartId, cancellationToken);

            return new OrderWithCommentDto
            {
                CartId = order.CartId,
                UserId = order.UserId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Items = order.Items,
                AdminComment = GetTemporaryAdminComment(cartId) // Логика временных комментариев
            };
        }

        private string? GetTemporaryAdminComment(int cartId)
        {
            // Временная реализация - можно заменить на реальную логику
            return cartId % 2 == 0 ? "Проверенный заказ" : null;
        }
    }
}