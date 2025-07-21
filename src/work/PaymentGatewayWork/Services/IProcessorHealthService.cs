using PaymentGateway.Common.Enum;

namespace PaymentGatewayWork.Services
{
    public interface IProcessorHealthService
    {
        public Task<bool> IsProcessorAvailableAsync(ProcessorType processorType, CancellationToken cancellationToken = default);
    }

}
