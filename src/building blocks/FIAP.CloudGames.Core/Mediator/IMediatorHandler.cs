using FIAP.CloudGames.Core.Messages;
using FluentValidation.Results;

namespace FIAP.CloudGames.Core.Mediator
{
    public interface IMediatorHandler
    {
        Task PublishEvent<T>(T evento) where T : Event;

        Task<ValidationResult> SendCommand<T>(T comando) where T : Command;
    }
}