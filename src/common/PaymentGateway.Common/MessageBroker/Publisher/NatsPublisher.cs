using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NATS.Client;
using PaymentGateway.Common.MessageBroker.Contracts;
using PaymentGateway.Common.MessageBroker.Publisher;
using System.Text.Json;
using System.Threading.Channels;

public class NatsPublisher : INatsPublisher, IDisposable
{
    private readonly ILogger<NatsPublisher> _logger;
    private readonly IConnection _connection;
    private readonly Channel<NatsMessage> _channel;
    private readonly CancellationTokenSource _cts = new();
    private readonly List<Task> _workers = new();
    private readonly int _workerCount = 2;
    
    public NatsPublisher(ILogger<NatsPublisher> logger, IConfiguration configuration)
    {
        _logger = logger;
        var options = ConnectionFactory.GetDefaultOptions();
        options.Url = configuration["Nats:Url"] ?? "nats://nats:4222";
        _connection = new ConnectionFactory().CreateConnection(options);

        _channel = Channel.CreateUnbounded<NatsMessage>(new UnboundedChannelOptions
        {
            SingleReader = false, 
            SingleWriter = false
        });

        for (int i = 0; i < _workerCount; i++)
        {
            _workers.Add(Task.Run(() => ProcessQueueAsync(i, _cts.Token), _cts.Token));
        }
    }

    public Task PublishAsync<T>(string subject, T message)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(message);
        var msg = new NatsMessage(subject, payload);

        if (!_channel.Writer.TryWrite(msg))
        {
            _logger.LogWarning("Falha ao enfileirar mensagem para o NATS.");
        }
        else
        {
            _logger.LogInformation($"Mensagem enfileirada para o NATS: {subject}");
        }

        return Task.CompletedTask;
    }

    private async Task ProcessQueueAsync(int workerId, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var msg in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    _connection.Publish(msg.Subject, msg.Payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{workerId} Erro ao publicar no NATS: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogError($"{workerId} cancelado.");
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _channel.Writer.Complete();
        Task.WaitAll(_workers.ToArray(), TimeSpan.FromSeconds(5));
        _connection.Dispose();
        _cts.Dispose();
    }
}
