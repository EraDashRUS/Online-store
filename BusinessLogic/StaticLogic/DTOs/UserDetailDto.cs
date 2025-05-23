using OnlineStore.BusinessLogic.StaticLogic.DTOs;

public class UserDetailDto : UserBriefDto
{
    public string Phone { get; set; }
    public string Address { get; set; }
  //  public bool IsAdmin { get; set; }

    public List<CartDto> ActiveCarts { get; set; } = new();
}