using PaymentGateway.Common.Enum;
using PaymentGateway.Common.Model;

namespace PaymentGateway.Common.Repository
{
    public interface IPaymentRepository
    {
        Task InsertAsync(Payment payment);
        Task<PaymentsSummaryAggregate> GetSummaryAsync(DateTime? from, DateTime? to);
        Task<Payment?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken);
        Task UpdateEntityAsync(Payment payment, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>?> GetPendingRetriesAsync(CancellationToken stoppingToken);
    }

}
