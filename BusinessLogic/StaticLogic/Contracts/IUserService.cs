using OnlineStore.BusinessLogic.StaticLogic.DTOs.User;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    public interface IUserService
    {
        Task<UserResponseDto> CreateUserAsync(UserCreateDto userDto, CancellationToken cancellationToken = default);
        Task<UserResponseDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<UserResponseDto> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);

    }
}
