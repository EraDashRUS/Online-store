namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class OrderBriefDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
