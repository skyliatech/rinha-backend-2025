using Microsoft.Extensions.Configuration;
using NATS.Client;
using System.Text.Json;

namespace PaymentGateway.Common.MessageBroker.Publisher
{
    public class NatsPublisher : INatsPublisher, IDisposable
    {
        private readonly IConnection _connection;

        public NatsPublisher(IConfiguration configuration)
        {
            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = configuration["Nats:Url"] ?? "nats://nats:4222";
            _connection = new ConnectionFactory().CreateConnection(options);
        }

        public Task PublishAsync<T>(string subject, T message)
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(message);
            _connection.Publish(subject, payload);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
