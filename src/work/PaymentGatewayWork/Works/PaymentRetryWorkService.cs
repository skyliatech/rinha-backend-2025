using PaymentGateway.Common.Enum;
using PaymentGateway.Common.Model;
using PaymentGateway.Common.Repository;
using PaymentGatewayWork.Rest.Base;
using PaymentGatewayWork.Services;


namespace PaymentGatewayWork.Works
{
    public class PaymentRetryWorkService : BackgroundService
    {
        private readonly ILogger<PaymentRetryWorkService> _logger;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IProcessorHealthService _healthService;
        private readonly IPaymentProcessorApi _defaultProcessor;
        private readonly IPaymentProcessorApi _fallbackProcessor;

        public PaymentRetryWorkService(
            ILogger<PaymentRetryWorkService> logger,
            IPaymentRepository paymentRepository,
            IProcessorHealthService healthService,
            IEnumerable<IPaymentProcessorApi> processors)
        {
            _logger = logger;
            _paymentRepository = paymentRepository;
            _healthService = healthService;
            _defaultProcessor = processors.First(p => p.Processor == ProcessorType.Default);
            _fallbackProcessor = processors.First(p => p.Processor == ProcessorType.Fallback);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de reprocessamento de pagamentos iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var payments = await _paymentRepository.GetPendingRetriesAsync(stoppingToken);

                if(payments?.Count() == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                _logger.LogInformation("Reprocessando {Count} pagamentos pendentes.", payments.Count());

                foreach (var payment in payments)
                {

                    var defaultAvailable = await _healthService.IsProcessorAvailableAsync(ProcessorType.Default, stoppingToken);
                    var fallbackAvailable = await _healthService.IsProcessorAvailableAsync(ProcessorType.Fallback, stoppingToken);

                    IPaymentProcessorApi? processor = null;

                    if (defaultAvailable)
                        processor = _defaultProcessor;
                    else if (fallbackAvailable)
                        processor = _fallbackProcessor;

                    if (processor is null)
                    {
                        _logger.LogWarning("Nenhum processador disponível para reprocessar {CorrelationId}. Tentaremos novamente depois.", payment.CorrelationId);
                        continue;
                    }

                    try
                    {
                        var success = await processor.ProcessAsync(payment, stoppingToken);
                        var status = success ? StatusPayment.Approved : StatusPayment.Failed;

                        payment.SetUpdatedPayment(processor.Processor, status);

                        await _paymentRepository.UpdateEntityAsync(payment, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao reprocessar pagamento {CorrelationId}", payment.CorrelationId);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

}
