using PaymentGateway.Common.Enum;

namespace PaymentGatewayWork.Rest.Base
{
    public interface IProcessorHealthCheckApi
    {
        ProcessorType Type { get; }
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    }

}
