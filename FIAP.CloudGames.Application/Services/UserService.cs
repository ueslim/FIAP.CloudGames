using FIAP.CloudGames.Application.DTOs;
using FIAP.CloudGames.Application.Interfaces;
using FIAP.CloudGames.Application.Mappings;
using FIAP.CloudGames.Domain.Interfaces;
using FluentValidation;

namespace FIAP.CloudGames.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IValidator<CreateUserDto> _createUserValidator;

        public UserService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IValidator<CreateUserDto> createUserValidator)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _createUserValidator = createUserValidator;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            await _createUserValidator.ValidateAndThrowAsync(createUserDto);

            var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);

            if (existingUser != null)
                throw new ValidationException("Email already in use");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
            var user = UserMappingService.MapToEntity(createUserDto);
            user.PasswordHash = passwordHash;

            var createdUser = await _userRepository.AddAsync(user);

            return UserMappingService.MapToDto(createdUser);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);

            if (user == null || !user.IsActive)
                throw new ValidationException("Invalid email or password");

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!isValidPassword)
                throw new ValidationException("Invalid email or password");

            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var token = _tokenService.GenerateToken(user);

            return new LoginResponseDto
            {
                Token = token,
                User = UserMappingService.MapToDto(user)
            };
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || !user.IsActive)
                throw new ValidationException("User not found");

            return UserMappingService.MapToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Where(u => u.IsActive).Select(UserMappingService.MapToDto);
        }

        public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || !user.IsActive)
                throw new ValidationException("User not found");

            if (!string.IsNullOrEmpty(updateUserDto.Name))
                user.Name = updateUserDto.Name;

            if (!string.IsNullOrEmpty(updateUserDto.Email))
            {
                var existingUser = await _userRepository.GetByEmailAsync(updateUserDto.Email);

                if (existingUser != null && existingUser.Id != id)
                    throw new ValidationException("Email already in use");

                user.Email = updateUserDto.Email;
            }

            if (!string.IsNullOrEmpty(updateUserDto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);

            if (updateUserDto.Role.HasValue)
                user.Role = updateUserDto.Role.Value;

            var updatedUser = await _userRepository.UpdateAsync(user);

            return UserMappingService.MapToDto(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || !user.IsActive)
                return false;

            user.IsActive = false;

            await _userRepository.UpdateAsync(user);

            return true;
        }
    }
}