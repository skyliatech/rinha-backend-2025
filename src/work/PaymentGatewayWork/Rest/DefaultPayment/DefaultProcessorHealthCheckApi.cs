using PaymentGateway.Common.Enum;
using PaymentGatewayWork.Rest.Base;

namespace PaymentGatewayWork.Rest.DefaultPayment
{
    public class DefaultProcessorHealthCheckApi : BaseProcessorHealthCheckApi
    {
        public override ProcessorType Type => ProcessorType.Default;

        public DefaultProcessorHealthCheckApi(IHttpClientFactory httpClientFactory, ILogger<BaseProcessorHealthCheckApi> logger)
            : base(httpClientFactory, nameof(DefaultProcessorHealthCheckApi), logger)
        {
        }
    }

 


}
