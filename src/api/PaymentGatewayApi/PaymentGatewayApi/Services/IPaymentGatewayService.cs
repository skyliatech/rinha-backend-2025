using PaymentGatewayApi.DataTransferObjects.Requests;
using PaymentGatewayApi.DataTransferObjects.Responses;

namespace PaymentGatewayApi.Services
{
    public interface IPaymentGatewayService
    {
        Task<PaymentsSummaryResponse> GetPaymentsSummaryAsync(DateTime? from, DateTime? to);
        Task<bool> SendPaymentAsync(PaymentRequest paymentRequest);
    }
}
