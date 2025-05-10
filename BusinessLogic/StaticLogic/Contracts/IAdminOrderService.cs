using OnlineStore.BusinessLogic.StaticLogic.DTOs;


namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    /// <summary>
    /// Интерфейс сервиса для административного управления заказами
    /// </summary>
    public interface IAdminOrderService
    {
        /// <summary>
        /// Подтверждает заказ администратором
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="adminComment">Комментарий администратора</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task ApproveOrderAsync(int cartId, string? adminComment, CancellationToken cancellationToken);

        /// <summary>
        /// Отклоняет заказ администратором
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="reason">Причина отклонения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task RejectOrderAsync(int cartId, string reason, CancellationToken cancellationToken);

        /// <summary>
        /// Получает статистику по заказам
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Статистика заказов</returns>
        Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken);
    }
}