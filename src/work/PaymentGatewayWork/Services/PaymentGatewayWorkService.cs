using PaymentGateway.Common.Enum;
using PaymentGateway.Common.MessageBroker.Contracts;
using PaymentGateway.Common.Model;
using PaymentGateway.Common.Repository;
using PaymentGatewayWork.Rest.Base;


namespace PaymentGatewayWork.Services
{
    public class PaymentGatewayWorkService : IPaymentGatewayWorkService
    {
        private readonly ILogger<PaymentGatewayWorkService> _logger;
        private readonly IPaymentProcessorApi _defaultProcessor;
        private readonly IPaymentProcessorApi _fallbackProcessor;
        private readonly IProcessorHealthService _healthService;
        private readonly IPaymentRepository _paymentRepository;

        public PaymentGatewayWorkService(
            ILogger<PaymentGatewayWorkService> logger,
            IEnumerable<IPaymentProcessorApi> processors,
            IProcessorHealthService healthService,
            IPaymentRepository paymentRepository)
        {
            _logger = logger;
            _defaultProcessor = processors.First(p => p.Processor == ProcessorType.Default);
            _fallbackProcessor = processors.First(p => p.Processor == ProcessorType.Fallback);
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
                _logger.LogInformation("Pagamento {CorrelationId} já está em status {Status}, não será reprocessado nesse fluxo.", payment.CorrelationId, payment.Status);
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
                payment.SetUpdatedPayment(ProcessorType.Unknown, StatusPayment.Failed);
                await _paymentRepository.UpdateEntityAsync(payment, cancellationToken);

                return;
            }

            try
            {
                var success = await processorToUse.ProcessAsync(payment, cancellationToken);
                var finalStatus = success ? StatusPayment.Approved : StatusPayment.Failed;
                payment.SetUpdatedPayment(processorToUse.Processor, finalStatus);
                await _paymentRepository.UpdateEntityAsync(payment, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pagamento {CorrelationId}", payment.CorrelationId);
                payment.SetUpdatedPayment(processorToUse.Processor, StatusPayment.Failed);
                await _paymentRepository.UpdateEntityAsync(payment, cancellationToken);
               
            }
        }
    }
}
