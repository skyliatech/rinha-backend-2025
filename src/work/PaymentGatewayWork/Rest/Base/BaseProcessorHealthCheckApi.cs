using PaymentGateway.Common.Enum;
using System.Text.Json;

namespace PaymentGatewayWork.Rest.Base
{
    public abstract class BaseProcessorHealthCheckApi : IProcessorHealthCheckApi
    {
        private readonly HttpClient _httpClient;
        public abstract ProcessorType Type { get; }

        protected BaseProcessorHealthCheckApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("/payments/service-health", cancellationToken);

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
