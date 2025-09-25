using FIAP.CloudGames.API.Controllers;
using FIAP.CloudGames.Application.DTOs;
using FIAP.CloudGames.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FIAP.CloudGames.Tests.Entities
{
    public class UserTest
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IGameService> _gameServiceMock;
        private readonly UsersController _controller;

        public UserTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _gameServiceMock = new Mock<IGameService>();
            _controller = new UsersController(_userServiceMock.Object, _gameServiceMock.Object);
        }

        [Fact(DisplayName = "Validando o retorno de todos os usuários")]
        [Trait("Categoria", "Seleção Usuário")]
        public async Task GetAll_ReturnsAllUsers()
        {
            var users = new List<UserDto> {
            new UserDto { Id = Guid.NewGuid(), Name = "User 1", Email = "u1@mail.com", Role = "User", IsActive = true },
            new UserDto { Id = Guid.NewGuid(), Name = "User 2", Email = "u2@mail.com", Role = "Admin", IsActive = true }
        };

            _userServiceMock.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count());
        }

        [Fact(DisplayName = "Validando o retorno de um único usuário")]
        [Trait("Categoria", "Seleção Usuário")]
        public async Task GetById_ReturnsUser_WhenFound()
        {
            var id = Guid.NewGuid();
            var user = new UserDto { Id = id, Name = "Test", Email = "test@mail.com", Role = "User", IsActive = true };

            _userServiceMock.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync(user);

            var result = await _controller.GetById(id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(id, returnedUser.Id);
        }

        [Fact(DisplayName = "Validando quando o usuário não é encontrado")]
        [Trait("Categoria", "Seleção Usuário")]
        public async Task GetById_ReturnsBadRequest_WhenNotFound()
        {
            var id = Guid.NewGuid();
            _userServiceMock.Setup(s => s.GetUserByIdAsync(id)).ThrowsAsync(new ValidationException("User not found"));

            var result = await _controller.GetById(id);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal("User not found", dict["message"]);
        }

        [Fact(DisplayName = "Validando o retorno da lista de games por usuário")]
        [Trait("Categoria", "Seleção Usuário")]
        public async Task GetUserLibrary_ReturnsGames()
        {
            var userId = Guid.NewGuid();
            var games = new List<GameDto> { new GameDto { Id = Guid.NewGuid(), Title = "Game 1" } };

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            _gameServiceMock.Setup(s => s.GetUserLibraryAsync(userId)).ReturnsAsync(games);

            var result = await _controller.GetUserLibrary();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedGames = Assert.IsAssignableFrom<IEnumerable<GameDto>>(okResult.Value);
            Assert.Single(returnedGames);
        }

        [Fact(DisplayName = "Quando o usuário não é encontrado")]
        [Trait("Categoria", "Atualização de usuário")]
        public async Task Update_ReturnsBadRequest_WhenUserNotFound()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateUserDto { Name = "New" };

            _userServiceMock.Setup(s => s.UpdateUserAsync(id, dto)).ThrowsAsync(new ApplicationException("User not found"));

            var result = await _controller.Update(id, dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal("User not found", dict["message"]);
        }

        [Fact(DisplayName = "Quando novo email já existe")]
        [Trait("Categoria", "Atualização de usuário")]
        public async Task Update_ReturnsBadRequest_WhenEmailExists()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateUserDto { Email = "email@email.com" };

            _userServiceMock.Setup(s => s.UpdateUserAsync(id, dto)).ThrowsAsync(new ApplicationException("Email already in use"));

            var result = await _controller.Update(id, dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal("Email already in use", dict["message"]);
        }

        [Fact(DisplayName = "Validando atualização de usuário com sucesso")]
        [Trait("Categoria", "Atualização de usuário")]
        public async Task Update_ReturnsOk_WhenSuccessful()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateUserDto { Name = "Updated" };

            _userServiceMock.Setup(s => s.UpdateUserAsync(id, dto)).ReturnsAsync(new UserDto { Id = id, Name = "Updated", Email = "email@mail.com", Role = "User", IsActive = true });

            var result = await _controller.Update(id, dto);

            Assert.IsType<OkResult>(result);
        }

        [Fact(DisplayName = "Validando exclusão de usuário com sucesso")]
        [Trait("Categoria", "Exclusão de usuário")]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            var id = Guid.NewGuid();
            _userServiceMock.Setup(s => s.DeleteUserAsync(id)).ReturnsAsync(true);

            var result = await _controller.Delete(id);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact(DisplayName = "Validando exclusão de usuário com erro")]
        [Trait("Categoria", "Exclusão de usuário")]
        public async Task Delete_ReturnsNotFound_WhenUserNotFound()
        {
            var id = Guid.NewGuid();
            _userServiceMock.Setup(s => s.DeleteUserAsync(id)).ReturnsAsync(false);

            var result = await _controller.Delete(id);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}