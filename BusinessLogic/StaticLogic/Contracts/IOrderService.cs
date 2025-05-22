using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using System.Threading;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    /// <summary>
    /// Интерфейс сервиса для работы с заказами
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Создает новый заказ на основе корзины
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO созданного заказа</returns>
        Task<OrderDto> CreateOrderFromCartAsync(int cartId, CancellationToken cancellationToken);

        /// <summary>
        /// Получает заказ по идентификатору корзины
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO заказа</returns>
        Task<OrderDto> GetOrderAsync(int cartId, CancellationToken cancellationToken);

        /// <summary>
        /// Получает список всех заказов
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Список DTO заказов</returns>
        Task<List<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Обновляет статус заказа
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="statusDto">DTO с новым статусом</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленный DTO заказа</returns>
        Task<OrderDto> UpdateOrderStatusAsync(int cartId, OrderStatusUpdateDto statusDto, CancellationToken cancellationToken);

        /// <summary>
        /// Обновляет данные заказа
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="orderDto">DTO с обновленными данными заказа</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленный DTO заказа</returns>
        Task<OrderDto> UpdateOrderAsync(int cartId, OrderDto orderDto, CancellationToken cancellationToken);

        /// <summary>
        /// Получает заказ с комментарием
        /// </summary>
        /// <param name="cartId">Идентификатор корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>DTO заказа с комментарием</returns>
        Task<OrderWithCommentDto> GetOrderWithCommentAsync(int cartId, CancellationToken cancellationToken);


        Task<AdminOrderDto> GetAdminOrderAsync(int cartId, CancellationToken cancellationToken);
    }
}
