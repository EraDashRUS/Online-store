using System.Collections.Generic;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    /// <summary>
    /// DTO для ответа с информацией о корзине
    /// </summary>
    public class CartResponseDto
    {
        /// <summary>
        /// Идентификатор корзины
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Список элементов корзины
        /// </summary>
        public List<CartItemResponseDto> Items { get; set; } = new();

        /// <summary>
        /// Общая стоимость корзины
        /// </summary>
        public decimal TotalPrice { get; set; }


        public string Status { get; set; }
    }
}
