using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.BusinessLogic.StaticLogic.Contracts.Exceptions;
using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using OnlineStore.Storage.Data;
using OnlineStore.Storage.Models;
using System.Threading;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    /// <summary>
    /// Сервис для администрирования заказов
    /// </summary>
    /// <remarks>
    /// Инициализирует новый экземпляр сервиса администрирования заказов
    /// </remarks>
    /// <param name="context">Контекст базы данных</param>
    /// <param name="productService">Сервис для работы с товарами</param>
    public class AdminOrderService(
        ApplicationDbContext context,
        IProductService productService) : IAdminOrderService
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IProductService _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        private readonly Dictionary<int, string> _tempComments = [];

        /// <summary>
        /// Подтверждает заказ
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="adminComment">Комментарий администратора</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <exception cref="NotFoundException">Заказ не найден</exception>
        public async Task ApproveOrderAsync(int cartId, string? adminComment, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
            if (cart == null) throw new NotFoundException("Заказ не найден");

            _tempComments[cartId] = adminComment;
            cart.Status = "Approved";
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Отклоняет заказ
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="adminComment">Комментарий администратора</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <exception cref="NotFoundException">Заказ не найден</exception>
        public async Task RejectOrderAsync(int cartId, string? adminComment, CancellationToken cancellationToken)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

            if (cart == null) throw new NotFoundException("Заказ не найден");

            cart.Status = "Rejected";
            _tempComments[cartId] = adminComment;
            await _context.SaveChangesAsync(cancellationToken);

            await RestockItemsAsync(cart, cancellationToken);
        }

        /// <summary>
        /// Получает статистику по заказам
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Статистика по заказам</returns>
        public async Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken)
        {
            return new OrderStatsDto
            {
                TotalOrders = await _context.Carts.CountAsync(c => c.Status != null, cancellationToken),
                PendingOrders = await _context.Carts.CountAsync(c => c.Status == "Pending", cancellationToken),
                TotalRevenue = await CalculateTotalRevenueAsync(cancellationToken)
            };
        }

        /// <summary>
        /// Возвращает товары на склад
        /// </summary>
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

        /// <summary>
        /// Рассчитывает общую выручку
        /// </summary>
        private async Task<decimal> CalculateTotalRevenueAsync(CancellationToken cancellationToken)
        {
            return await _context.Carts
                .Where(c => c.Status == "Approved")
                .SelectMany(c => c.CartItems)
                .SumAsync(i => i.Quantity * i.Product.Price, cancellationToken);
        }

        /// <summary>
        /// Получает комментарий администратора для заказа
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <returns>Комментарий администратора</returns>
        public string? GetAdminComment(int cartId)
        {
            return _tempComments.TryGetValue(cartId, out var comment) ? comment : null;
        }
    }
}
