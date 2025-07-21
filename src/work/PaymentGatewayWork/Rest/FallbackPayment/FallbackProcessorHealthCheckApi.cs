using PaymentGateway.Common.Enum;
using PaymentGatewayWork.Rest.Base;

namespace PaymentGatewayWork.Rest.FallbackPayment
{
    public class FallbackProcessorHealthCheckApi : BaseProcessorHealthCheckApi
    {
        public override ProcessorType Type => ProcessorType.Fallback;

        public FallbackProcessorHealthCheckApi(HttpClient httpClient)
            : base(httpClient)
        {
        }
    }
}
