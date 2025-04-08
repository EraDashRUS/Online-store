using OnlineStore.DTOs;

namespace OnlineStore.Contracts
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartByUserIdAsync(int userId);
        Task<CartItemResponseDto> AddToCartAsync(CartItemCreateDto itemDto);
        Task<bool> RemoveFromCartAsync(int itemId);
        Task<bool> ClearCartAsync(int userId);
        Task<CartResponseDto> UpdateCartItemAsync(CartItemUpdateDto itemDto);
    }
}