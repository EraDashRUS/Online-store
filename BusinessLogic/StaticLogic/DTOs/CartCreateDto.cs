namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    public class CartCreateDto  // Новый DTO
    {
        public int UserId { get; set; }  // Для админа/менеджера
        public List<CartItemCreateDto> Items { get; set; }
    }
}
