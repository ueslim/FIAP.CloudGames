using FIAP.CloudGames.Application.DTOs;
using FIAP.CloudGames.Domain.Entities;

namespace FIAP.CloudGames.Application.Mappings
{
    public static class GameMappingService
    {
        public static GameDto MapToDto(Game game)
        {
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
                Tags = game.Tags
            };
        }

        public static Game MapToEntity(CreateGameDto dto)
        {
            return new Game
            {
                Title = dto.Title,
                Description = dto.Description,
                Developer = dto.Developer,
                Publisher = dto.Publisher,
                ReleaseDate = dto.ReleaseDate,
                Price = dto.Price,
                CoverImageUrl = dto.CoverImageUrl,
                Tags = dto.Tags,
                IsActive = true
            };
        }

        public static IEnumerable<GameDto> MapToDtoCollection(IEnumerable<Game> games)
        {
            return games.Select(MapToDto).ToList();
        }
    }
}