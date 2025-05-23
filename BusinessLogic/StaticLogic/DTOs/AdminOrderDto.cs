using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class AdminOrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; }

        public string Status { get; set; } // Гарантированно не-null
        public List<OrderItemDto> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
