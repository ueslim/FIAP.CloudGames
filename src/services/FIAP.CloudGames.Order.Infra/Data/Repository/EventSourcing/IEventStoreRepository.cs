using FIAP.CloudGames.Core.Events;

namespace FIAP.CloudGames.Order.Infra.Data.Repository.EventSourcing
{
    public interface IEventStoreRepository : IDisposable
    {
        void Store(StoredEvent theEvent);

        Task<IList<StoredEvent>> All(Guid aggregateId);
    }
}