using PaymentGateway.Common.Enum;
using PaymentGateway.Common.Model;
using System.Text;
using System.Text.Json;

namespace PaymentGatewayWork.Rest.Base
{
    public abstract class BasePaymentProcessorApi : IPaymentProcessorApi
    {
        private readonly ILogger _logger;

        private readonly string _name;

        private readonly IHttpClientFactory _httpClientFactory;
        public abstract ProcessorType Processor { get; }

        protected BasePaymentProcessorApi(IHttpClientFactory httpClientFactory, string name, ILogger<BasePaymentProcessorApi> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _name = name;
        }

        public async Task<bool> ProcessAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            try
            {
                var payload = new
                {
                    correlationId = payment.CorrelationId,
                    amount = payment.Amount,
                    requestedAt = payment.RequestedAt.ToUniversalTime().ToString("O") 
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var _httpClient = _httpClientFactory.CreateClient(_name);
                var response = await _httpClient.PostAsync("/payments", content, cancellationToken);
                string json = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("ProcessAsync: {Processor} - {StatusCode} - {Response}", Processor.ToString(), (int)response.StatusCode, json);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

}
