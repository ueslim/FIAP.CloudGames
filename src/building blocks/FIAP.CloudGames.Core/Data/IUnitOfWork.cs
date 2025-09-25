namespace FIAP.CloudGames.Core.Data
{
    public interface IUnitOfWork
    {
        Task<bool> Commit();
    }
}