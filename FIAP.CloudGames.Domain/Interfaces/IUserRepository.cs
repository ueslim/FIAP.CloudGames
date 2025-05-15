using FIAP.CloudGames.Domain.Entities;

namespace FIAP.CloudGames.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);

        Task<User> GetByEmailAsync(string email);

        Task<IEnumerable<User>> GetAllAsync();

        Task<User> AddAsync(User user);

        Task<User> UpdateAsync(User user);

        Task<bool> DeleteAsync(Guid id);
    }
}