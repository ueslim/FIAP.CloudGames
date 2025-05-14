using FIAP.CloudGames.Application.DTOs;

namespace FIAP.CloudGames.Application.Interfaces
{
    public interface IGameService
    {
        Task<GameDto> GetGameByIdAsync(Guid id);

        Task<IEnumerable<GameDto>> GetAllGamesAsync();

        Task<IEnumerable<GameDto>> GetAllGamesWithDapperAsync();

        Task<GameDto> CreateGameAsync(CreateGameDto createGameDto);

        Task<GameDto> UpdateGameAsync(Guid id, UpdateGameDto updateGameDto);

        Task<bool> DeleteGameAsync(Guid id);

        Task<IEnumerable<GameDto>> SearchGamesAsync(string term, string[] tags, decimal? minPrice, decimal? maxPrice);

        Task<GameDto> PurchaseGameAsync(Guid userId, PurchaseGameDto purchaseDto);

        Task<IEnumerable<GameDto>> GetUserLibraryAsync(Guid userId);

        Task<IEnumerable<GameDto>> GetUserLibraryWithDapperAsync(Guid userId);
    }
}