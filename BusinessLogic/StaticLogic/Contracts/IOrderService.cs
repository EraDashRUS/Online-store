using OnlineStore.BusinessLogic.StaticLogic.DTOs.Admin;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.Order;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    public interface IOrderService
    {

        Task<OrderDto> CreateOrderFromCartAsync(int cartId, CancellationToken cancellationToken);


        Task<OrderDto> GetOrderAsync(int cartId, CancellationToken cancellationToken);

        Task<List<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken);

        Task<OrderDto> UpdateOrderStatusAsync(int cartId, OrderStatusUpdateDto statusDto, CancellationToken cancellationToken);

        Task<OrderDto> UpdateOrderAsync(int cartId, OrderDto orderDto, CancellationToken cancellationToken);

        Task<OrderWithCommentDto> GetOrderWithCommentAsync(int cartId, CancellationToken cancellationToken);


        Task<AdminOrderDto> GetAdminOrderAsync(int cartId, CancellationToken cancellationToken);
    }
}
