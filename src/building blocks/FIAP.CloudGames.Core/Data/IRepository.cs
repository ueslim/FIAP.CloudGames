using FIAP.CloudGames.Core.DomainObjects;

namespace FIAP.CloudGames.Core.Data
{
    public interface IRepository<T> : IDisposable where T : IAggregateRoot
    {
        IUnitOfWork UnitOfWork { get; }
    }
}