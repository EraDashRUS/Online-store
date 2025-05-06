using OnlineStore.BusinessLogic.StaticLogic.DTOs;
using System.Threading;
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
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные корзины пользователя</returns>
        Task<CartResponseDto> GetCartByUserIdAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавляет товар в корзину
        /// </summary>
        /// <param name="itemDto">Данные добавляемого товара</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Данные добавленного товара в корзине</returns>
        Task<CartItemResponseDto> AddToCartAsync(CartItemCreateDto itemDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет товар из корзины
        /// </summary>
        /// <param name="itemId">Идентификатор элемента корзины</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True если элемент успешно удален, иначе false</returns>
        Task<bool> RemoveFromCartAsync(int itemId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Очищает корзину пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True если корзина успешно очищена, иначе false</returns>
        Task<bool> ClearCartAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновляет элемент корзины
        /// </summary>
        /// <param name="itemDto">Данные обновления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Обновленные данные корзины</returns>
        Task<CartResponseDto> UpdateCartItemAsync(CartItemUpdateDto itemDto, CancellationToken cancellationToken = default);
    }
}