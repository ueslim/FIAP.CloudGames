using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Domain.Interfaces;
using FIAP.CloudGames.Infra.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Infra.Repository
{
    public class GameRepository : BaseRepository<Game>, IGameRepository
    {
        public GameRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Game>> GetGamesByIdsAsync(IEnumerable<Guid> ids)
        {
            return await _dbSet.Where(g => ids.Contains(g.Id) && g.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Game>> SearchGamesAsync(string term, string[] tags, decimal? minPrice, decimal? maxPrice)
        {
            var query = _dbSet.Where(g => g.IsActive);

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(g =>
                    g.Title.Contains(term) ||
                    g.Description.Contains(term));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(g => g.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(g => g.Price <= maxPrice.Value);
            }

            // Executa a parte que o EF consegue converter em SQL
            var result = await query.ToListAsync();

            // Agora filtra por Tags em memória
            if (tags != null && tags.Length > 0)
            {
                result = result
                    .Where(g => g.Tags.Any(t => tags.Contains(t)))
                    .ToList();
            }

            return result;
        }
    }
}