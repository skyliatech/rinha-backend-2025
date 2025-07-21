using PaymentGateway.Common.Enum;

namespace PaymentGateway.Common.Model
{
    public class Payment
    {
        public string CorrelationId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime RequestedAt { get; set; }
        public ProcessorType ProcessorUsed { get; set; }
        public StatusPayment Status { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public bool Processed { get; set; }

        public Payment(string correlationId, decimal amount, DateTime requestedAt)
        {
            CorrelationId = correlationId;
            Amount = amount;
            RequestedAt = requestedAt;
            CreatedAt = DateTime.UtcNow;
            Status = StatusPayment.Pending;
            Processed = false;
        }

        public void WorkAsProcessed(ProcessorType processorUsed, StatusPayment status)
        {
            ProcessorUsed = processorUsed;
            Status = status;
            Processed = true;
            ProcessedAt = DateTime.UtcNow;
        }

    }

    public enum StatusPayment
    {
        Pending = 0,
        Approved = 1,
        Failed = 2
    }

    
}
