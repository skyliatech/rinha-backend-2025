using PaymentGateway.Common.Enum;
using PaymentGatewayWork.Rest.Base;

namespace PaymentGatewayWork.Rest.DefaultPayment
{
    public class DefaultPaymentProcessorApi : BasePaymentProcessorApi
    {
        public override ProcessorType Processor => ProcessorType.Default;

        public DefaultPaymentProcessorApi(IHttpClientFactory httpClientFactory, ILogger<BasePaymentProcessorApi> logger) 
            : base(httpClientFactory, nameof(DefaultPaymentProcessorApi), logger) { }
    }
}
