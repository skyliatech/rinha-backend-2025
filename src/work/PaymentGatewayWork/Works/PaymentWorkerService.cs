
using PaymentGateway.Common.MessageBroker.Contracts;
using PaymentGateway.Common.MessageBroker.Subscriber;
using PaymentGatewayWork.Services;

namespace PaymentGatewayWork.Works
{
    public class PaymentWorkerService : BackgroundService
    {
        private readonly ILogger<PaymentWorkerService> _logger;
        private readonly INatsSubscriber _natsSubscriber;
        private readonly IPaymentGatewayWorkService _paymentGatewayWorkService;

        public PaymentWorkerService(
            ILogger<PaymentWorkerService> logger,
            INatsSubscriber natsSubscriber,
            IPaymentGatewayWorkService paymentGatewayWorkService)
        {
            _logger = logger;
            _natsSubscriber = natsSubscriber;
            _paymentGatewayWorkService = paymentGatewayWorkService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker iniciado. Inscrevendo no tópico 'payment.requested'...");

            _natsSubscriber.Subscribe<PaymentRequestedMessage>("payment.requested", async message =>
            {
                try
                {
                    await _paymentGatewayWorkService.HandleAsync(message, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem PaymentRequestedMessage.");
                }
            });

            return Task.CompletedTask;
        }
    }


}
