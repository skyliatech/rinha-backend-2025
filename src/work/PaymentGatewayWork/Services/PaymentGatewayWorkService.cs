using PaymentGateway.Common.MessageBroker.Contracts;
using PaymentGateway.Common.Model;
using PaymentGateway.Common.Repository;
using PaymentGatewayWork.Enum;
using PaymentGatewayWork.Rest;
using PaymentGatewayWork.Rest.DefaultPayment;
using PaymentGatewayWork.Rest.FallbackPayment;


namespace PaymentGatewayWork.Services
{
    public class PaymentGatewayWorkService : IPaymentGatewayWorkService
    {
        private readonly ILogger<PaymentGatewayWorkService> _logger;
        private readonly IDefaultPaymentProcessorApi _defaultProcessor;
        private readonly IFallbackPaymentProcessorApi _fallbackProcessor;
        private readonly IProcessorHealthService _healthService;
        private readonly IPaymentRepository _paymentRepository;

        public PaymentGatewayWorkService(
            ILogger<PaymentGatewayWorkService> logger,
            IDefaultPaymentProcessorApi defaultProcessor,
            IFallbackPaymentProcessorApi fallbackProcessor,
            IProcessorHealthService healthService,
            IPaymentRepository paymentRepository)
        {
            _logger = logger;
            _defaultProcessor = defaultProcessor;
            _fallbackProcessor = fallbackProcessor;
            _healthService = healthService;
            _paymentRepository = paymentRepository;
        }
        public async Task HandleAsync(PaymentRequestedMessage message, CancellationToken cancellationToken = default)
        {
           
            var payment = await _paymentRepository.GetByCorrelationIdAsync(message.CorrelationId, cancellationToken);

            if (payment is null)
            {
                _logger.LogWarning("Pagamento {CorrelationId} não encontrado no banco.", message.CorrelationId);
                return;
            }

            if (payment.Status !=  StatusPayment.Pending)
            {
                _logger.LogInformation("Pagamento {CorrelationId} já está em status {Status}, não será reprocessado.", payment.CorrelationId, payment.Status);
                return;
            }

            var defaultAvailable = await _healthService.IsProcessorAvailableAsync(ProcessorType.Default);
            var fallbackAvailable = await _healthService.IsProcessorAvailableAsync(ProcessorType.Fallback);

            IPaymentProcessorApi? processorToUse = null;

            if (defaultAvailable)
                processorToUse = _defaultProcessor;
            else if (fallbackAvailable)
                processorToUse = _fallbackProcessor;

            if (processorToUse is null)
            {
                _logger.LogWarning("Nenhum processador disponível. Pagamento {CorrelationId} marcado como FAILED.", payment.CorrelationId);
                await _paymentRepository.UpdateStatusAsync(payment.CorrelationId, StatusPayment.Failed, cancellationToken);
                return;
            }

            try
            {
                var success = await processorToUse.ProcessAsync(payment, cancellationToken);
                var finalStatus = success ? StatusPayment.Approved : StatusPayment.Failed;

                await _paymentRepository.UpdateStatusAsync(payment.CorrelationId, finalStatus, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pagamento {CorrelationId}", payment.CorrelationId);
                await _paymentRepository.UpdateStatusAsync(payment.CorrelationId, StatusPayment.Failed, cancellationToken);
            }
        }
    }
}
