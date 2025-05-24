namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.Order
{
    public class OrderDto
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderStatusUpdateDto
    {
        public string NewStatus { get; set; }
        public string AdminComment { get; set; }
    }
}