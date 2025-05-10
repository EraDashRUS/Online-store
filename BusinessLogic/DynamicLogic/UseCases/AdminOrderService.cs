using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Data;
using OnlineStore.Models;
using System.Threading;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        private readonly Dictionary<int, string> _tempComments = new();

        public AdminOrderService(
            ApplicationDbContext context,
            IProductService productService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        public async Task ApproveOrderAsync(int cartId, string? adminComment, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
            if (cart == null) throw new NotFoundException("Заказ не найден");

            _tempComments[cartId] = adminComment; // Сохраняем в памяти
            cart.Status = "Approved";
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RejectOrderAsync(int cartId, string? adminComment, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

            if (cart == null) throw new NotFoundException("Заказ не найден");

            cart.Status = "Rejected";
            _tempComments[cartId] = adminComment;
            await _context.SaveChangesAsync(cancellationToken);

            // Возвращаем товары на склад
            await RestockItemsAsync(cart, cancellationToken);
        }

        public async Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken)
        {
            return new OrderStatsDto
            {
                TotalOrders = await _context.Carts.CountAsync(c => c.Status != null, cancellationToken),
                PendingOrders = await _context.Carts.CountAsync(c => c.Status == "Pending", cancellationToken),
                TotalRevenue = await CalculateTotalRevenueAsync(cancellationToken)
            };
        }

        private async Task RestockItemsAsync(Cart cart, CancellationToken cancellationToken)
        {
            foreach (var item in cart.Items)
            {
                await _productService.ReduceStockAsync(
                    item.ProductId,
                    -item.Quantity, // Отрицательное значение для возврата
                    cancellationToken);
            }
        }

        private async Task<decimal> CalculateTotalRevenueAsync(CancellationToken cancellationToken)
        {
            return await _context.Carts
                .Where(c => c.Status == "Approved")
                .SelectMany(c => c.Items)
                .SumAsync(i => i.Quantity * i.Product.Price, cancellationToken);
        }

        public string? GetAdminComment(int cartId)
        {
            return _tempComments.TryGetValue(cartId, out var comment) ? comment : null;
        }

    }
}
