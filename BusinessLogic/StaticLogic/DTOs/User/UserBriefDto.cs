using OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs.User
{
    public class UserBriefDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public List<CartBriefDto> ActiveCarts { get; set; } = new();
    }
}
