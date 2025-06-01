using FIAP.CloudGames.API.Controllers;
using FIAP.CloudGames.Application.DTOs;
using FIAP.CloudGames.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace FIAP.CloudGames.Tests.Entities
{
    public class GameTest
    {
        private readonly Mock<IGameService> _gameServiceMock;
        private readonly GamesController _controller;

        public GameTest()
        {
            _gameServiceMock = new Mock<IGameService>();
            _controller = new GamesController(_gameServiceMock.Object);
        }

        [Fact(DisplayName = "Validando o retorno de um único jogo")]
        [Trait("Categoria", "Seleção de jogos")]
        public async Task GetById_ReturnsGame_WhenFound()
        {
            var gameId = Guid.NewGuid();
            var expectedGame = new GameDto { Id = gameId, Title = "Test", Description = "Desc" };

            _gameServiceMock.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(expectedGame);

            var result = await _controller.GetById(gameId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedGame, okResult.Value);
        }

        [Fact(DisplayName = "Validando o retorno de todos os jogos")]
        [Trait("Categoria", "Seleção de jogos")]
        public async Task GetAll_ReturnsAllGames()
        {
            var games = new List<GameDto> { new() { Id = Guid.NewGuid(), Title = "Game" } };
            _gameServiceMock.Setup(s => s.GetAllGamesAsync()).ReturnsAsync(games);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(games, okResult.Value);
        }

        [Fact(DisplayName = "Validando quando o jogo não é encontrado")]
        [Trait("Categoria", "Seleção de jogos")]
        public async Task GetById_ReturnsBadRequest_WhenNotFound()
        {
            var gameId = Guid.NewGuid();
            _gameServiceMock.Setup(s => s.GetGameByIdAsync(gameId)).ThrowsAsync(new ValidationException("Game not found"));

            var result = await _controller.GetById(gameId);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal("Game not found", dict["message"]);
        }

        [Fact(DisplayName = "Validando atualização de jogos com sucesso")]
        [Trait("Categoria", "Atualização de jogos")]
        public async Task Update_ReturnsOk_WhenAdmin()
        {
            var gameId = Guid.NewGuid();
            var updateDto = new UpdateGameDto { Title = "Updated" };
            var expected = new GameDto { Id = gameId, Title = "Updated" };
            _gameServiceMock.Setup(s => s.UpdateGameAsync(gameId, updateDto)).ReturnsAsync(expected);

            var result = await _controller.Update(gameId, updateDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact(DisplayName = "Validando atualização de jogos com erro")]
        [Trait("Categoria", "Atualização de jogos")]
        public async Task Update_ReturnsBadRequest_WhenUser()
        {
            var gameId = Guid.NewGuid();
            var updateDto = new UpdateGameDto();
            _gameServiceMock.Setup(s => s.UpdateGameAsync(gameId, updateDto)).ThrowsAsync(new ValidationException("Permission denied"));

            var result = await _controller.Update(gameId, updateDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal("Permission denied", dict["message"]);
        }

        [Fact(DisplayName = "Validando exclusão de jogos com sucesso")]
        [Trait("Categoria", "Exclusão de jogos")]
        public async Task Delete_ReturnsNoContent_WhenAdmin()
        {
            var gameId = Guid.NewGuid();
            _gameServiceMock.Setup(s => s.DeleteGameAsync(gameId)).ReturnsAsync(true);

            var result = await _controller.Delete(gameId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact(DisplayName = "Validando exclusão de jogos com erro")]
        [Trait("Categoria", "Exclusão de jogos")]
        public async Task Delete_ReturnsBadRequest_WhenUser()
        {
            var gameId = Guid.NewGuid();
            _gameServiceMock.Setup(s => s.DeleteGameAsync(gameId)).ThrowsAsync(new ValidationException("Unauthorized"));

            var result = await _controller.Delete(gameId);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal("Unauthorized", dict["message"]);
        }

        [Fact(DisplayName = "Validando criação de jogos com sucesso")]
        [Trait("Categoria", "Criação de jogos")]
        public async Task Create_ReturnsCreated_WhenAdmin()
        {
            var createDto = new CreateGameDto { Title = "Game", Description = "Desc", Developer = "Dev", Publisher = "Pub", ReleaseDate = DateTime.Now, Price = 10, CoverImageUrl = "url", Tags = new[] { "tag" }, Genre = "Genre" };
            var expected = new GameDto { Id = Guid.NewGuid(), Title = "Game" };
            _gameServiceMock.Setup(s => s.CreateGameAsync(createDto)).ReturnsAsync(expected);

            var result = await _controller.Create(createDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(expected, createdResult.Value);
        }

        [Fact(DisplayName = "Validando criação de jogos com erro")]
        [Trait("Categoria", "Criação de jogos")]
        public async Task Create_ReturnsBadRequest_WhenUser()
        {
            var createDto = new CreateGameDto { Title = "Game", Description = "Desc", Developer = "Dev", Publisher = "Pub", ReleaseDate = DateTime.Now, Price = 10, CoverImageUrl = "url", Tags = new[] { "tag" }, Genre = "Genre" };
            _gameServiceMock.Setup(s => s.CreateGameAsync(createDto)).ThrowsAsync(new ValidationException("Unauthorized"));

            var result = await _controller.Create(createDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal("Unauthorized", dict["message"]);
        }

        [Theory]
        [Trait("Categoria", "Criação de jogos")]
        [InlineData("", "Description", 10)]
        [InlineData("Title", "", 10)]
        [InlineData("Title", "Description", 0)]
        public async Task Create_ReturnsBadRequest_WhenFieldsAreInvalid(string title, string description, decimal price)
        {
            var createDto = new CreateGameDto
            {
                Title = title,
                Description = description,
                Developer = "Dev",
                Publisher = "Pub",
                ReleaseDate = DateTime.Now,
                Price = price,
                CoverImageUrl = "url",
                Tags = new[] { "tag" },
                Genre = "Genre"
            };

            _gameServiceMock.Setup(s => s.CreateGameAsync(createDto)).ThrowsAsync(new ValidationException("Validation failed"));

            var result = await _controller.Create(createDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequest.Value)
            );

            Assert.Equal("Validation failed", dict["message"]);
        }
    }
}
