using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using System.Threading.Tasks;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    /// <summary>
    /// Интерфейс сервиса для работы с корзиной
    /// </summary>
    public interface ICartService
    {
        /// <summary>
        /// Получает корзину пользователя по его идентификатору
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Данные корзины пользователя</returns>
        Task<CartResponseDto> GetCartByUserIdAsync(int userId);

        /// <summary>
        /// Добавляет товар в корзину
        /// </summary>
        /// <param name="itemDto">Данные добавляемого товара</param>
        /// <returns>Данные добавленного товара в корзине</returns>
        Task<CartItemResponseDto> AddToCartAsync(CartItemCreateDto itemDto);

        /// <summary>
        /// Удаляет товар из корзины
        /// </summary>
        /// <param name="itemId">Идентификатор элемента корзины</param>
        /// <returns>True если элемент успешно удален, иначе false</returns>
        Task<bool> RemoveFromCartAsync(int itemId);

        /// <summary>
        /// Очищает корзину пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>True если корзина успешно очищена, иначе false</returns>
        Task<bool> ClearCartAsync(int userId);

        /// <summary>
        /// Обновляет элемент корзины
        /// </summary>
        /// <param name="itemDto">Данные обновления</param>
        /// <returns>Обновленные данные корзины</returns>
        Task<CartResponseDto> UpdateCartItemAsync(CartItemUpdateDto itemDto);
    }
}