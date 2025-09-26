using FIAP.CloudGames.Core.Events;
using FIAP.CloudGames.Core.Messages;
using FIAP.CloudGames.Order.Infra.Data.Repository.EventSourcing;
using FIAP.CloudGames.WebAPI.Core.User;
using Newtonsoft.Json;

namespace FIAP.CloudGames.Order.Infra.Data.EventSourcing
{
    public class SqlEventStore : IEventStore
    {
        private readonly IEventStoreRepository _eventStoreRepository;
        private readonly IAspNetUser _user;

        public SqlEventStore(IEventStoreRepository eventStoreRepository, IAspNetUser user)
        {
            _eventStoreRepository = eventStoreRepository;
            _user = user;
        }

        public void Save<T>(T theEvent) where T : Event
        {
            var serializedData = JsonConvert.SerializeObject(theEvent);

            var userName = _user?.Name;
            if (string.IsNullOrEmpty(userName))
                userName = _user?.GetUserEmail();
            if (string.IsNullOrEmpty(userName))
                userName = "system"; // fallback default

            var storedEvent = new StoredEvent(theEvent, serializedData, userName);

            _eventStoreRepository.Store(storedEvent);
        }
    }
}