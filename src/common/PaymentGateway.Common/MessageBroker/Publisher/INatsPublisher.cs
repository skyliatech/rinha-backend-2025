namespace PaymentGateway.Common.MessageBroker.Publisher
{
    public interface INatsPublisher
    {
        Task PublishAsync<T>(string subject, T message);
    }
}
