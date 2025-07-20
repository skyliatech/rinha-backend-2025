

namespace PaymentGateway.Common.MessageBroker.Contracts
{
    public class PaymentRequestedMessage
    {
        public string CorrelationId { get; set; } = default!;
        public decimal Amount { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}
