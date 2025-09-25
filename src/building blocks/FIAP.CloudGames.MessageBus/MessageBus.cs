using EasyNetQ;
using FIAP.CloudGames.Core.Messages.Integration;

namespace FIAP.CloudGames.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private IBus _bus;
        private readonly string _connectionString;

        public MessageBus(string connectionString)
        {
            _connectionString = connectionString;
            TryConnect();
        }

        public bool IsConnected => _bus?.Advanced?.IsConnected ?? false;

        public IAdvancedBus AdvancedBus => _bus?.Advanced;

        public void Publish<T>(T message) where T : IntegrationEvent
        {
            TryConnect();
            _bus.PubSub.PublishAsync(message).GetAwaiter().GetResult();
        }

        public async Task PublishAsync<T>(T message) where T : IntegrationEvent
        {
            TryConnect();
            await _bus.PubSub.PublishAsync(message);
        }

        public void Subscribe<T>(string subscriptionId, Action<T> onMessage) where T : class
        {
            TryConnect();
            _bus.PubSub.SubscribeAsync<T>(subscriptionId, msg =>
            {
                onMessage(msg);
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();
        }

        public void SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage) where T : class
        {
            TryConnect();
            _bus.PubSub.SubscribeAsync(subscriptionId, onMessage).GetAwaiter().GetResult();
        }

        public TResponse Request<TRequest, TResponse>(TRequest request)
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage
        {
            TryConnect();
            return _bus.Rpc.RequestAsync<TRequest, TResponse>(request).GetAwaiter().GetResult();
        }

        public async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request)
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage
        {
            TryConnect();
            return await _bus.Rpc.RequestAsync<TRequest, TResponse>(request);
        }

        public IDisposable Respond<TRequest, TResponse>(Func<TRequest, TResponse> responder)
      where TRequest : IntegrationEvent
      where TResponse : ResponseMessage
        {
            TryConnect();
            // RespondAsync retorna AwaitableDisposable<IDisposable>
            // pegamos o IDisposable resultante de forma síncrona
            var registration = _bus.Rpc.RespondAsync<TRequest, TResponse>(req =>
                Task.FromResult(responder(req)));

            return registration.GetAwaiter().GetResult();
        }

        public IDisposable RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder)
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage
        {
            TryConnect();
            var registration = _bus.Rpc.RespondAsync(responder);

            return registration.GetAwaiter().GetResult();
        }

        private void TryConnect()
        {
            if (_bus != null && _bus.Advanced.IsConnected) return;

            // v7 continua suportando RabbitHutch.CreateBus(connectionString)
            _bus = RabbitHutch.CreateBus(_connectionString);
            // Em v7 o EasyNetQ já cuida de reconexão; você pode,
            // se quiser, observar eventos em _bus.Advanced.Connected/Disconnected.
        }

        public void Dispose()
        {
            _bus?.Dispose();
        }
    }
}