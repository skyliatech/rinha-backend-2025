using PaymentGateway.Common.Enum;
using System.Text.Json;

namespace PaymentGatewayWork.Rest.Base
{
    public abstract class BaseProcessorHealthCheckApi : IProcessorHealthCheckApi
    {
        private readonly ILogger<BaseProcessorHealthCheckApi> _logger;

        private readonly string _name;

        private readonly IHttpClientFactory _httpClientFactory;
        public abstract ProcessorType Type { get; }

        protected BaseProcessorHealthCheckApi(IHttpClientFactory httpClientFactory, string name, ILogger<BaseProcessorHealthCheckApi> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _name = name;
        }

        public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var _httpClient = _httpClientFactory.CreateClient(_name);

                var response = await _httpClient.GetAsync("/payments/service-health", cancellationToken);
                string json = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("IsHealthyAsync: {Type} - {StatusCode} - {Response}", Type.ToString(), (int)response.StatusCode, json);

                if (!response.IsSuccessStatusCode)
                    return false;

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var health = JsonSerializer.Deserialize<ServiceHealthResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return health is { Failing: false };
            }
            catch
            {
                return false;
            }
        }
       
    }

}
