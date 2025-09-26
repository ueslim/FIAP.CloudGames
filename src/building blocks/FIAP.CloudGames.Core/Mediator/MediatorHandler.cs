using FIAP.CloudGames.Core.Events;
using FIAP.CloudGames.Core.Messages;
using FluentValidation.Results;
using MediatR;

namespace FIAP.CloudGames.Core.Mediator
{
    public class MediatorHandler : IMediatorHandler
    {
        private readonly IMediator _mediator;
        private readonly IEventStore _eventStore;

        public MediatorHandler(IMediator mediator, IEventStore eventStore)
        {
            _mediator = mediator;
            _eventStore = eventStore;
        }

        public async Task<ValidationResult> SendCommand<T>(T command) where T : Command
        {
            return await _mediator.Send(command);
        }

        public async Task PublishEvent<T>(T events) where T : Event
        {
            _eventStore?.Save(events);
            await _mediator.Publish(events);
        }
    }
}