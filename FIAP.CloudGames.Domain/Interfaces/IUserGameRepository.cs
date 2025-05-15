using FIAP.CloudGames.Domain.Entities;

namespace FIAP.CloudGames.Domain.Interfaces
{
    public interface IUserGameRepository
    {
        Task<UserGame> GetByIdAsync(Guid id);

        Task<IEnumerable<UserGame>> GetByUserIdAsync(Guid userId);

        Task<UserGame> AddAsync(UserGame userGame);

        Task<bool> ExistsAsync(Guid userId, Guid gameId);
    }
}