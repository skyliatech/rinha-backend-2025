using PaymentGateway.Common.MessageBroker.Contracts;

namespace PaymentGatewayWork.Services
{
    public interface IPaymentGatewayWorkService
    {
        public Task HandleAsync(PaymentRequestedMessage message, CancellationToken cancellationToken = default);
    }
}
