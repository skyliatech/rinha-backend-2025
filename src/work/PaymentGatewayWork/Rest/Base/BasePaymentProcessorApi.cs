using PaymentGateway.Common.Enum;
using PaymentGateway.Common.Model;
using System.Text;
using System.Text.Json;

namespace PaymentGatewayWork.Rest.Base
{
    public abstract class BasePaymentProcessorApi : IPaymentProcessorApi
    {
        private readonly HttpClient _httpClient;
        public abstract ProcessorType Processor { get; }

        protected BasePaymentProcessorApi(HttpClient httpClient)
        {   
            _httpClient = httpClient;
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

                var response = await _httpClient.PostAsync("/payments", content, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

}
