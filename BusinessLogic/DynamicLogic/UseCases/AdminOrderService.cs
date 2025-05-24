using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Order;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    /// <summary>
    /// Сервис для административного управления заказами: подтверждение, отклонение, статистика, комментарии.
    /// </summary>
    public class AdminOrderService(ApplicationDbContext context, IProductService productService) : IAdminOrderService
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IProductService _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        private readonly Dictionary<int, string> _tempComments = [];

        /// <summary>
        /// Подтверждение заказа с возможностью добавить комментарий администратора.
        /// </summary>
        /// <param name="cartId">ID корзины</param>
        /// <param name="adminComment">Комментарий администратора</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <exception cref="NotFoundException">Если заказ не найден</exception>
        public async Task ApproveOrderAsync(int cartId, string? adminComment, CancellationToken cancellationToken)
        {
            var cart = await GetCartByIdAsync(cartId, cancellationToken);
            _tempComments[cartId] = adminComment;
            cart.Status = "Approved";
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Отклонение заказа с возвратом товаров на склад и комментарием администратора.
        /// </summary>
        /// <param name="cartId">ID корзины</param>
        /// <param name="adminComment">Комментарий администратора</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <exception cref="NotFoundException">Если заказ не найден</exception>
        public async Task RejectOrderAsync(int cartId, string? adminComment, CancellationToken cancellationToken)
        {
            var cart = await GetCartWithItemsByIdAsync(cartId, cancellationToken);
            cart.Status = "Rejected";
            _tempComments[cartId] = adminComment;
            await _context.SaveChangesAsync(cancellationToken);
            await RestockItemsAsync(cart, cancellationToken);
        }

        /// <summary>
        /// Получить статистику по заказам: всего, ожидающих, выручка.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Статистика заказов</returns>
        public async Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken)
        {
            var totalOrdersTask = _context.Carts.CountAsync(c => c.Status != null, cancellationToken);
            var pendingOrdersTask = _context.Carts.CountAsync(c => c.Status == "Pending", cancellationToken);
            var totalRevenueTask = CalculateTotalRevenueAsync(cancellationToken);

            await Task.WhenAll(totalOrdersTask, pendingOrdersTask, totalRevenueTask);

            return new OrderStatsDto
            {
                TotalOrders = totalOrdersTask.Result,
                PendingOrders = pendingOrdersTask.Result,
                TotalRevenue = totalRevenueTask.Result
            };
        }

        /// <summary>
        /// Получить комментарий администратора к заказу.
        /// </summary>
        /// <param name="cartId">ID корзины</param>
        /// <returns>Комментарий или null</returns>
        public string? GetAdminComment(int cartId)
        {
            return _tempComments.TryGetValue(cartId, out var comment) ? comment : null;
        }

        private async Task<Cart> GetCartByIdAsync(int cartId, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
            if (cart == null)
                throw new NotFoundException("Заказ не найден");
            return cart;
        }

        private async Task<Cart> GetCartWithItemsByIdAsync(int cartId, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
            if (cart == null)
                throw new NotFoundException("Заказ не найден");
            return cart;
        }

        private async Task RestockItemsAsync(Cart cart, CancellationToken cancellationToken)
        {
            foreach (var item in cart.CartItems)
            {
                await _productService.ReduceStockAsync(
                    item.ProductId,
                    -item.Quantity,
                    cancellationToken);
            }
        }

        private async Task<decimal> CalculateTotalRevenueAsync(CancellationToken cancellationToken)
        {
            return await _context.Carts
                .Where(c => c.Status == "Approved")
                .SelectMany(c => c.CartItems)
                .SumAsync(i => i.Quantity * i.Product.Price, cancellationToken);
        }
    }
}
