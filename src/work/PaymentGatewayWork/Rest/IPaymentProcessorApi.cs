using PaymentGateway.Common.Model;


namespace PaymentGatewayWork.Rest
{
    public interface IPaymentProcessorApi
    {
        string Name { get; }
        Task<bool> ProcessAsync(Payment payment, CancellationToken cancellationToken = default);
    }
}
