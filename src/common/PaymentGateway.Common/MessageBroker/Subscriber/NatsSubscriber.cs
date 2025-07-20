using Microsoft.Extensions.Configuration;
using NATS.Client;
using System.Text.Json;

namespace PaymentGateway.Common.MessageBroker.Subscriber
{
    public class NatsSubscriber : INatsSubscriber, IDisposable
    {
        private readonly IConnection _connection;

        public NatsSubscriber(IConfiguration configuration)
        {
            var opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = configuration["Nats:Url"] ?? "nats://nats:4222";
            _connection = new ConnectionFactory().CreateConnection(opts);
        }

        public void Subscribe<T>(string subject, Func<T, Task> onMessage)
        {
            _connection.SubscribeAsync(subject, async (sender, args) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<T>(args.Message.Data);
                    if (message is not null)
                    {
                        await onMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar mensagem do NATS: {ex.Message}");
                }
            });
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
