using PaymentGateway.Common.Enum;
using PaymentGateway.Common.Model;


namespace PaymentGatewayWork.Rest
{
    public interface IPaymentProcessorApi
    {
        ProcessorType Processor { get; }
        Task<bool> ProcessAsync(Payment payment, CancellationToken cancellationToken = default);
    }
}
