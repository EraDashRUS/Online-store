namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.Order
{
    public class OrderBriefDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
