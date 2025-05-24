using OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.User;

public class UserDetailDto : UserBriefDto
{
    public string Phone { get; set; }
    public string Address { get; set; }

    public List<CartDto> ActiveCarts { get; set; } = new();
}