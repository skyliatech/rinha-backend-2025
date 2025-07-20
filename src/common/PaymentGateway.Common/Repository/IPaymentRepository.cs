using PaymentGateway.Common.Model;


namespace PaymentGateway.Common.Repository
{
    public interface IPaymentRepository
    {
        Task InsertAsync(Payment payment);
        Task UpdateAfterProcessingAsync(string correlationId, string processorUsed, StatusPayment status);
        Task<PaymentsSummaryAggregate> GetSummaryAsync(DateTime? from, DateTime? to);
    }

}
