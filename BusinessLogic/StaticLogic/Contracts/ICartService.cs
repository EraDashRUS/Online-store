using OnlineStore.BusinessLogic.StaticLogic.DTOs.Cart;
using OnlineStore.BusinessLogic.StaticLogic.DTOs.CartItem;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{

    public interface ICartService
    {

        Task<CartResponseDto> GetCartByUserIdAsync(int userId, CancellationToken cancellationToken = default);


        Task<CartItemResponseDto> AddToCartAsync(CartItemCreateDto itemDto, CancellationToken cancellationToken = default);


        Task<bool> RemoveFromCartAsync(int itemId, CancellationToken cancellationToken = default);

        Task<bool> ClearCartAsync(int userId, CancellationToken cancellationToken = default);

        Task<CartResponseDto> UpdateCartItemAsync(CartItemUpdateDto itemDto, CancellationToken cancellationToken = default);
    }
}