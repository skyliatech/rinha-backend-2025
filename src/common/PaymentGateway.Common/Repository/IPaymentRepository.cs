using PaymentGateway.Common.Enum;
using PaymentGateway.Common.Model;

namespace PaymentGateway.Common.Repository
{
    public interface IPaymentRepository
    {
        Task InsertAsync(Payment payment);
        Task UpdateAfterProcessingAsync(string correlationId, ProcessorType processorUsed, StatusPayment status, CancellationToken cancellationToken);
        Task<PaymentsSummaryAggregate> GetSummaryAsync(DateTime? from, DateTime? to);
        Task<Payment> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken);
        Task UpdateStatusAsync(string correlationId, StatusPayment statusPayment, CancellationToken cancellationToken);
    }

}
