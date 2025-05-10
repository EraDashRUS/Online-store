using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using System.Threading;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    public interface IOrderService
    {
        // Создание заказа из корзины (новый метод)
        Task<OrderDto> CreateOrderFromCartAsync(int cartId, CancellationToken cancellationToken);

        // Получение "заказа" (фактически корзины со статусом)
        Task<OrderDto> GetOrderAsync(int cartId, CancellationToken cancellationToken);

        // Получение всех "заказов" (корзин с определенными статусами)
        Task<List<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken);

        // Обновление статуса
        Task<OrderDto> UpdateOrderStatusAsync(int cartId, OrderStatusUpdateDto statusDto, CancellationToken cancellationToken);

        // Обновление элементов заказа (не рекомендуется, лучше создавать новый)
        Task<OrderDto> UpdateOrderAsync(int cartId, OrderDto orderDto, CancellationToken cancellationToken);

        Task<OrderWithCommentDto> GetOrderWithCommentAsync(int cartId, CancellationToken cancellationToken);
    }
}
