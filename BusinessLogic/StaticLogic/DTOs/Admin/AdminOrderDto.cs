using OnlineStore.BusinessLogic.StaticLogic.DTOs.Order;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.Admin
{
    public class AdminOrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; }

        public string Status { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
