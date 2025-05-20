using FIAP.CloudGames.Domain.Entities;

namespace FIAP.CloudGames.Domain.Interfaces
{
    public interface IDapperGameRepository
    {
        Task<IEnumerable<Game>> GetAllGamesWithDapperAsync();

        Task<IEnumerable<Game>> GetUserLibraryWithDapperAsync(Guid userId);
    }
}