using FIAP.CloudGames.Domain.Entities;

namespace FIAP.CloudGames.Domain.Interfaces
{
    public interface IGameRepository
    {
        Task<Game> GetByIdAsync(Guid id);

        Task<IEnumerable<Game>> GetAllAsync();

        Task<IEnumerable<Game>> GetGamesByIdsAsync(IEnumerable<Guid> ids);

        Task<Game> AddAsync(Game game);

        Task<Game> UpdateAsync(Game game);

        Task<bool> DeleteAsync(Guid id);

        Task<IEnumerable<Game>> SearchGamesAsync(string term, string[] tags, decimal? minPrice, decimal? maxPrice);
    }
}