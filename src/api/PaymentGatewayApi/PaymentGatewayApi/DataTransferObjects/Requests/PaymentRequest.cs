using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PaymentGatewayApi.DataTransferObjects.Requests
{
    public class PaymentRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Campo correlationId é obrigatório!")]
        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; }

        [Required(ErrorMessage = "Campo amount é obrigatório!")]
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }
}
