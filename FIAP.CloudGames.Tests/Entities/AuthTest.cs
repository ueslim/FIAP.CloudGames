using FIAP.CloudGames.Application.DTOs;
using FIAP.CloudGames.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FIAP.CloudGames.API.Controllers;
using Xunit;
using Moq;
using FIAP.CloudGames.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace FIAP.CloudGames.Tests.Entities
{
    public class AuthTest
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly AuthController _controller;

        public AuthTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new AuthController(_userServiceMock.Object);
        }

        [Fact(DisplayName = "Validando se o login foi bem sucedido")]
        [Trait("Categoria", "Login")]
        public async Task Login_ReturnsOk_WhenLoginIsSuccessful()
        {
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var expectedResponse = new LoginResponseDto
            {
                Token = "mocked-token",
                User = new UserDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Test User",
                    Email = "test@example.com",
                    Role = "User",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            _userServiceMock
                .Setup(s => s.LoginAsync(loginDto))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.Login(loginDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact(DisplayName = "Validando login errado")]
        [Trait("Categoria", "Login")]
        public async Task Login_ReturnsBadRequest_WhenEmailOrPasswordInvalid()
        {
            var loginDto = new LoginDto
            {
                Email = "invalid@example.com",
                Password = "WrongPassword!"
            };

            _userServiceMock
                .Setup(s => s.LoginAsync(loginDto))
                .ThrowsAsync(new ValidationException("Invalid email or password"));

            var result = await _controller.Login(loginDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal("Invalid email or password", dict["message"]);
        }

        [Fact(DisplayName = "Validando criação de usuário com sucesso")]
        [Trait("Categoria", "Criação de usuário")]
        public async Task Register_ReturnsCreated_WhenUserIsValid()
        {
            var createUserDto = new CreateUserDto
            {
                Name = "New User",
                Email = "newuser@example.com",
                Password = "Password123!",
                Role = UserRole.User
            };

            var expectedUser = new UserDto
            {
                Id = Guid.NewGuid(),
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                Role = createUserDto.Role.ToString(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _userServiceMock
                .Setup(s => s.CreateUserAsync(createUserDto))
                .ReturnsAsync(expectedUser);

            var result = await _controller.Register(createUserDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(expectedUser, createdResult.Value);
        }

        [Fact(DisplayName = "Validando email já existente")]
        [Trait("Categoria", "Criação de usuário")]
        public async Task Register_ReturnsBadRequest_WhenEmailAlreadyExists()
        {
            var createUserDto = new CreateUserDto
            {
                Name = "Existing User",
                Email = "existing@example.com",
                Password = "Password123!",
                Role = UserRole.User
            };

            _userServiceMock
                .Setup(s => s.CreateUserAsync(createUserDto))
                .ThrowsAsync(new ValidationException("Email already in use"));

            var result = await _controller.Register(createUserDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal("Email already in use", dict["message"]);
        }

        [Theory]
        [Trait("Categoria", "Criação de usuário")]
        [InlineData("Jo", "Name must be at least 3 characters")]
        [InlineData(null, "Name is required")]
        public async Task Register_ReturnsBadRequest_WhenNameIsInvalid(string name, string expectedMessage)
        {
            name ??= "";

            var createUserDto = new CreateUserDto
            {
                Name = name,
                Email = "valid@example.com",
                Password = "Password123!",
                Role = UserRole.User
            };

            _userServiceMock
                .Setup(s => s.CreateUserAsync(createUserDto))
                .ThrowsAsync(new ValidationException(expectedMessage));

            var result = await _controller.Register(createUserDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal(expectedMessage, dict["message"]);
        }

        [Fact(DisplayName = "Validando nome muito longo")]
        [Trait("Categoria", "Criação de usuário")]
        public async Task Register_ReturnsBadRequest_WhenNameIsTooLong()
        {
            var longName = "A" + new string('a', 100); // 101 chars

            var createUserDto = new CreateUserDto
            {
                Name = longName,
                Email = "valid@example.com",
                Password = "Password123!",
                Role = UserRole.User
            };

            _userServiceMock
                .Setup(s => s.CreateUserAsync(createUserDto))
                .ThrowsAsync(new ValidationException("Name cannot exceed 100 characters"));

            var result = await _controller.Register(createUserDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal("Name cannot exceed 100 characters", dict["message"]);
        }

        [Theory]
        [Trait("Categoria", "Criação de usuário")]
        [InlineData("invalid-email", "Invalid email format")]
        [InlineData("", "Email is required")]
        public async Task Register_ReturnsBadRequest_WhenEmailIsInvalid(string email, string expectedMessage)
        {
            var createUserDto = new CreateUserDto
            {
                Name = "User",
                Email = email,
                Password = "Password123!",
                Role = UserRole.User
            };

            _userServiceMock
                .Setup(s => s.CreateUserAsync(createUserDto))
                .ThrowsAsync(new ValidationException(expectedMessage));

            var result = await _controller.Register(createUserDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal(expectedMessage, dict["message"]);
        }

        [Theory]
        [Trait("Categoria", "Criação de usuário")]
        [InlineData("short", "Password must be at least 8 characters")]
        [InlineData("password", "Password must contain at least one letter, one number, and one special character")]
        [InlineData("", "Password is required")]
        public async Task Register_ReturnsBadRequest_WhenPasswordIsInvalid(string password, string expectedMessage)
        {
            var createUserDto = new CreateUserDto
            {
                Name = "User",
                Email = "user@example.com",
                Password = password,
                Role = UserRole.User
            };

            _userServiceMock
                .Setup(s => s.CreateUserAsync(createUserDto))
                .ThrowsAsync(new ValidationException(expectedMessage));

            var result = await _controller.Register(createUserDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal(expectedMessage, dict["message"]);
        }
    }
}