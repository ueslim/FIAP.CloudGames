using FIAP.CloudGames.Application.DTOs;
using FIAP.CloudGames.Domain.Entities;

namespace FIAP.CloudGames.Application.Mappings
{
    public static class UserMappingService
    {
        public static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                IsActive = user.IsActive
            };
        }

        public static User MapToEntity(CreateUserDto dto)
        {
            return new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        public static IEnumerable<UserDto> MapToDtoCollection(IEnumerable<User> users)
        {
            return users.Select(MapToDto).ToList();
        }
    }
}