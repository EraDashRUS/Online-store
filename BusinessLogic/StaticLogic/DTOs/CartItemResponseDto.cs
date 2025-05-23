using System.Text.Json.Serialization;

namespace OnlineStore.BusinessLogic.StaticLogic.DTOs
{
    /// <summary>
    /// DTO для ответа с информацией об элементе корзины
    /// </summary>
    public class CartItemResponseDto
    {
        /// <summary>
        /// Идентификатор элемента корзины
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор корзины
        /// </summary>
        
        public int CartId { get; set; }

        /// <summary>
        /// Идентификатор товара
        /// </summary>
        public int ProductId { get; set; }

        public CartBriefDto Cart { get; set; }
        /// <summary>
        /// Количество товара
        /// </summary>
        public int Quantity { get; set; }

        public ProductBriefDto Product { get; set; }

    }
}