using PaymentGateway.Common.Enum;
using StackExchange.Redis;


namespace PaymentGatewayWork.Services
{
    public class RedisProcessorHealthService : IProcessorHealthService
    {
        private readonly IDatabase _redisDatabase;

        public RedisProcessorHealthService(IConnectionMultiplexer redis)
        {
            _redisDatabase = redis.GetDatabase();
            
        }
        public async Task<bool> IsProcessorAvailableAsync(ProcessorType processorType, CancellationToken cancellationToken = default)
        {
            var key = $"health:processor:{processorType.ToString().ToLower()}";
            return await _redisDatabase.KeyExistsAsync(key);
        }
    }
}
