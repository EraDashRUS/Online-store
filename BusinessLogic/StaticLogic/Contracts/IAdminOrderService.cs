using OnlineStore.BusinessLogic.StaticLogic.DTOs.Order;


namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    public interface IAdminOrderService
    {

        Task ApproveOrderAsync(int cartId, string? adminComment, CancellationToken cancellationToken);
        Task RejectOrderAsync(int cartId, string reason, CancellationToken cancellationToken);
        Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken);
    }
}