using OnlineStore.BusinessLogic.StaticLogic.DTOs;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    public interface IUserService
    {
        Task<UserResponseDto> CreateUserAsync(UserCreateDto userDto);
        Task<UserResponseDto> GetUserByIdAsync(int id);
    }
}
