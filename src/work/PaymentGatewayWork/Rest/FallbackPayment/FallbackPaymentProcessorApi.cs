using PaymentGateway.Common.Enum;
using PaymentGatewayWork.Rest.Base;


namespace PaymentGatewayWork.Rest.FallbackPayment
{
    public class FallbackPaymentProcessorApi : BasePaymentProcessorApi
    {
        public override ProcessorType Processor => ProcessorType.Fallback;

        public FallbackPaymentProcessorApi(IHttpClientFactory httpClientFactory, ILogger<BasePaymentProcessorApi> logger) 
            : base(httpClientFactory, nameof(FallbackPaymentProcessorApi), logger) { }
    }
}
