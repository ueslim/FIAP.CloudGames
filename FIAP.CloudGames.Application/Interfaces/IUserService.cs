using FIAP.CloudGames.Application.DTOs;

namespace FIAP.CloudGames.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        Task<UserDto> GetUserByIdAsync(Guid id);

        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);

        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);

        Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);

        Task<bool> DeleteUserAsync(Guid id);
    }
}