using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Domain.Interfaces;
using FIAP.CloudGames.Infra.Repository.Base;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Infra.Repository
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}