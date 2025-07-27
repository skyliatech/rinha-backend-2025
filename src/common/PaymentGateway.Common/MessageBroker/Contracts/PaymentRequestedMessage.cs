

namespace PaymentGateway.Common.MessageBroker.Contracts
{
    public record PaymentRequestedMessage
    {
        public string CorrelationId { get; set; } = default!;
        public decimal Amount { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}
