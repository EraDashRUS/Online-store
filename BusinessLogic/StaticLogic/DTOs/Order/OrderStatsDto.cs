namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.Order
{
    public class OrderStatsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ApprovedOrders { get; set; }
        public int RejectedOrders { get; set; }
    }
}