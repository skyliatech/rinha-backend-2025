using PaymentGateway.Common.MessageBroker.Contracts;
using PaymentGateway.Common.MessageBroker.Publisher;
using PaymentGateway.Common.Model;
using PaymentGateway.Common.Repository;
using PaymentGatewayApi.DataTransferObjects.Requests;
using PaymentGatewayApi.DataTransferObjects.Responses;

namespace PaymentGatewayApi.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly INatsPublisher _natsPublisher;
        public PaymentGatewayService(IPaymentRepository paymentRepository, INatsPublisher natsPublisher)
        {
            _paymentRepository = paymentRepository;
            _natsPublisher = natsPublisher;
        }

        public async Task<PaymentsSummaryResponse> GetPaymentsSummaryAsync(DateTime? from, DateTime? to)
        {
            var response =  await _paymentRepository.GetSummaryAsync(from, to);

            return new PaymentsSummaryResponse 
            { 
                Default = new DataTransferObjects.Responses.Default 
                { 
                    TotalAmount = response?.Default?.TotalAmount ?? default, 
                    TotalRequests = response?.Default?.TotalRequests ?? default
                },
                Fallback = new DataTransferObjects.Responses.Fallback 
                { 
                    TotalAmount = response?.Fallback?.TotalAmount ?? default, 
                    TotalRequests = response?.Fallback?.TotalRequests ?? default
                }
            };
        }

        public async Task<bool> SendPaymentAsync(PaymentRequest paymentRequest)
        {
            var payment = new Payment( paymentRequest.CorrelationId, paymentRequest.Amount, DateTime.UtcNow);

            await _paymentRepository.InsertAsync(payment);

            var message = new PaymentRequestedMessage
            {
                CorrelationId = payment.CorrelationId,
                Amount = payment.Amount,
                RequestedAt = payment.RequestedAt
            };

            await _natsPublisher.PublishAsync("payment.requested", message);

            return true;
        }
    }
}
