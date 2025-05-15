using FIAP.CloudGames.Domain.Entities;

namespace FIAP.CloudGames.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}