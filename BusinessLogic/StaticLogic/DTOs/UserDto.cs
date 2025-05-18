namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        // Активные корзины пользователя
        public List<CartDto> ActiveCarts { get; set; } = new();
    }
}
