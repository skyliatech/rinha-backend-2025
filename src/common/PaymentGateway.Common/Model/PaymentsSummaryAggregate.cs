
namespace PaymentGateway.Common.Model
{
    public class PaymentsSummaryAggregate
    {
        public Default Default { get; set; }

        public Fallback Fallback { get; set; }
    }

    public class Default
    {
        public int TotalRequests { get; set; }

        public decimal TotalAmount { get; set; }
    }

    public class Fallback
    {
        public int TotalRequests { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
