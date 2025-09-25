using FIAP.CloudGames.Application.DTOs;
using FIAP.CloudGames.Application.Interfaces;
using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FIAP.CloudGames.Application.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IUserGameRepository _userGameRepository;
        private readonly IDapperGameRepository _dapperGameRepository;

        public GameService(
            IGameRepository gameRepository,
            IUserGameRepository userGameRepository,
            IDapperGameRepository dapperGameRepository)
        {
            _gameRepository = gameRepository;
            _userGameRepository = userGameRepository;
            _dapperGameRepository = dapperGameRepository;
        }

        public async Task<GameDto> GetGameByIdAsync(Guid id)
        {
            var game = await _gameRepository.GetByIdAsync(id);

            if (game == null || !game.IsActive)
                throw new ValidationException("Game not found");

            return new GameDto
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                Developer = game.Developer,
                Publisher = game.Publisher,
                ReleaseDate = game.ReleaseDate,
                Price = game.Price,
                CoverImageUrl = game.CoverImageUrl,
                Tags = game.Tags,
                Genre = game.Genre
            };
        }

        public async Task<IEnumerable<GameDto>> GetAllGamesAsync()
        {
            var games = await _gameRepository.GetAllAsync();

            return games.Select(g => new GameDto
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                Developer = g.Developer,
                Publisher = g.Publisher,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                CoverImageUrl = g.CoverImageUrl,
                Tags = g.Tags,
                Genre = g.Genre
            });
        }

        // Using Dapper for high-performance queries
        public async Task<IEnumerable<GameDto>> GetAllGamesWithDapperAsync()
        {
            var games = await _dapperGameRepository.GetAllGamesWithDapperAsync();

            return games.Select(g => new GameDto
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                Developer = g.Developer,
                Publisher = g.Publisher,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                CoverImageUrl = g.CoverImageUrl,
                Tags = g.Tags,
                Genre = g.Genre
            });
        }

        public async Task<GameDto> CreateGameAsync(CreateGameDto createGameDto)
        {
            var game = new Game
            {
                Title = createGameDto.Title,
                Description = createGameDto.Description,
                Developer = createGameDto.Developer,
                Publisher = createGameDto.Publisher,
                ReleaseDate = createGameDto.ReleaseDate,
                Price = createGameDto.Price,
                CoverImageUrl = createGameDto.CoverImageUrl,
                Tags = createGameDto.Tags,
                IsActive = true,
                Genre = createGameDto.Genre
            };

            var createdGame = await _gameRepository.AddAsync(game);

            return new GameDto
            {
                Id = createdGame.Id,
                Title = createdGame.Title,
                Description = createdGame.Description,
                Developer = createdGame.Developer,
                Publisher = createdGame.Publisher,
                ReleaseDate = createdGame.ReleaseDate,
                Price = createdGame.Price,
                CoverImageUrl = createdGame.CoverImageUrl,
                Tags = createdGame.Tags,
                Genre = createdGame.Genre
            };
        }

        public async Task<GameDto> UpdateGameAsync(Guid id, UpdateGameDto updateGameDto)
        {
            var game = await _gameRepository.GetByIdAsync(id);

            if (game == null || !game.IsActive)
                throw new ValidationException("Game not found");

            if (updateGameDto.Title != null) game.Title = updateGameDto.Title;
            if (updateGameDto.Description != null) game.Description = updateGameDto.Description;
            if (updateGameDto.Developer != null) game.Developer = updateGameDto.Developer;
            if (updateGameDto.Publisher != null) game.Publisher = updateGameDto.Publisher;
            if (updateGameDto.ReleaseDate.HasValue) game.ReleaseDate = updateGameDto.ReleaseDate.Value;
            if (updateGameDto.Price.HasValue) game.Price = updateGameDto.Price.Value;
            if (updateGameDto.CoverImageUrl != null) game.CoverImageUrl = updateGameDto.CoverImageUrl;
            if (updateGameDto.Tags != null) game.Tags = updateGameDto.Tags;
            if (updateGameDto.Genre != null) game.Genre = updateGameDto.Genre;

            var updatedGame = await _gameRepository.UpdateAsync(game);

            return new GameDto
            {
                Id = updatedGame.Id,
                Title = updatedGame.Title,
                Description = updatedGame.Description,
                Developer = updatedGame.Developer,
                Publisher = updatedGame.Publisher,
                ReleaseDate = updatedGame.ReleaseDate,
                Price = updatedGame.Price,
                CoverImageUrl = updatedGame.CoverImageUrl,
                Tags = updatedGame.Tags,
                Genre = updatedGame.Genre
            };
        }

        public async Task<bool> DeleteGameAsync(Guid id)
        {
            return await _gameRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<GameDto>> SearchGamesAsync(string term, string[] tags, decimal? minPrice, decimal? maxPrice)
        {
            var games = await _gameRepository.SearchGamesAsync(term, tags, minPrice, maxPrice);

            return games.Select(g => new GameDto
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                Developer = g.Developer,
                Publisher = g.Publisher,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                CoverImageUrl = g.CoverImageUrl,
                Tags = g.Tags,
                Genre = g.Genre
            });
        }

        public async Task<GameDto> PurchaseGameAsync(Guid userId, PurchaseGameDto purchaseDto)
        {
            var game = await _gameRepository.GetByIdAsync(purchaseDto.GameId);

            if (game == null || !game.IsActive)
                throw new ValidationException("Game not found");

            var alreadyOwned = await _userGameRepository.ExistsAsync(userId, purchaseDto.GameId);

            if (alreadyOwned)
                throw new ValidationException("User already owns this game");

            var userGame = new UserGame
            {
                UserId = userId,
                GameId = purchaseDto.GameId,
                PurchasePrice = game.Price,
                IsActive = true
            };

            await _userGameRepository.AddAsync(userGame);

            return new GameDto
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                Developer = game.Developer,
                Publisher = game.Publisher,
                ReleaseDate = game.ReleaseDate,
                Price = game.Price,
                CoverImageUrl = game.CoverImageUrl,
                Tags = game.Tags,
                Genre = game.Genre
            };
        }

        public async Task<IEnumerable<GameDto>> GetUserLibraryAsync(Guid userId)
        {
            var userGames = await _userGameRepository.GetByUserIdAsync(userId);
            var gameIds = userGames.Select(ug => ug.GameId);
            var games = await _gameRepository.GetGamesByIdsAsync(gameIds);

            return games.Select(g => new GameDto
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                Developer = g.Developer,
                Publisher = g.Publisher,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                CoverImageUrl = g.CoverImageUrl,
                Tags = g.Tags,
                Genre = g.Genre
            });
        }

        // Using Dapper for high-performance queries
        public async Task<IEnumerable<GameDto>> GetUserLibraryWithDapperAsync(Guid userId)
        {
            var games = await _dapperGameRepository.GetUserLibraryWithDapperAsync(userId);

            return games.Select(g => new GameDto
            {
                Id = g.Id,
                Title = g.Title,
                Description = g.Description,
                Developer = g.Developer,
                Publisher = g.Publisher,
                ReleaseDate = g.ReleaseDate,
                Price = g.Price,
                CoverImageUrl = g.CoverImageUrl,
                Tags = g.Tags,
                Genre = g.Genre
            });
        }
    }
}