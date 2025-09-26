using FIAP.CloudGames.Core.Events;
using FIAP.CloudGames.Core.Messages;

public class NoOpEventStore : IEventStore
{
    public void Save<T>(T theEvent) where T : Event
    { }
}