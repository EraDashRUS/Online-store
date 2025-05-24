using OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.User
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public List<CartDto> ActiveCarts { get; set; } = new();
        public bool IsAdmin { get; set; }
    }
}