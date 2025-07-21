using PaymentGatewayWork.Rest.Base;
using StackExchange.Redis;

namespace PaymentGatewayWork.Works
{
    public class ProcessorHealthCheckBackgroundService : BackgroundService
    {
        private readonly IEnumerable<IProcessorHealthCheckApi> _healthCheckApis;
        private readonly IDatabase _redis;
        private readonly ILogger<ProcessorHealthCheckBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(5);

        public ProcessorHealthCheckBackgroundService(
            IEnumerable<IProcessorHealthCheckApi> healthCheckApis,
            IConnectionMultiplexer redis,
            ILogger<ProcessorHealthCheckBackgroundService> logger)
        {
            _healthCheckApis = healthCheckApis;
            _redis = redis.GetDatabase();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var api in _healthCheckApis)
                {
                    try
                    {
                        var isHealthy = await api.IsHealthyAsync(stoppingToken);
                        var key = $"health:processor:{api.Type.ToString().ToLower()}";

                        if (isHealthy)
                        {
                            await _redis.StringSetAsync(key, "healthy", Ttl);
                            _logger.LogDebug("Processador {Type} está saudável.", api.Type);
                        }
                        else
                        {
                            await _redis.KeyDeleteAsync(key);
                            _logger.LogWarning("Processador {Type} está indisponível.", api.Type);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao validar saúde do processador {Type}", api.Type);
                    }
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }
    }

}
