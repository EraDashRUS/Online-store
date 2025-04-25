namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int CartId { get; set; } // Связь с корзиной
        public List<int> CartIds { get; set; } // Список ID корзин
    }
}