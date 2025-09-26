

using FIAP.CloudGames.Core.Messages;

namespace FIAP.CloudGames.Core.Events
{
    public interface IEventStore
    {
        void Save<T>(T theEvent) where T : Event;
    }
}