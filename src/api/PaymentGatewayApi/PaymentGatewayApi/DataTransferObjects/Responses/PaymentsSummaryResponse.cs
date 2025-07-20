using System.Text.Json.Serialization;

namespace PaymentGatewayApi.DataTransferObjects.Responses
{
    public class PaymentsSummaryResponse
    {
        [JsonPropertyName("default")]
        public Default Default { get; set; }

        [JsonPropertyName("fallback")]
        public Fallback Fallback { get; set; }
    }

    public class Default
    {
        [JsonPropertyName("totalRequests")]
        public int TotalRequests { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }
    }

    public class Fallback
    {
        [JsonPropertyName("totalRequests")]
        public int TotalRequests { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }
    }
}
