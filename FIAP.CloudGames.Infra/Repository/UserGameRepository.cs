using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Domain.Interfaces;
using FIAP.CloudGames.Infra.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Infra.Repository
{
    public class UserGameRepository : BaseRepository<UserGame>, IUserGameRepository
    {
        public UserGameRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserGame>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(ug => ug.UserId == userId && ug.IsActive)
                .Include(ug => ug.Game)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid gameId)
        {
            return await _dbSet.AnyAsync(ug =>
                ug.UserId == userId &&
                ug.GameId == gameId &&
                ug.IsActive);
        }
    }
}