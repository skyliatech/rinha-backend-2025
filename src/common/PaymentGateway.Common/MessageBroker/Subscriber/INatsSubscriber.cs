

namespace PaymentGateway.Common.MessageBroker.Subscriber
{
    public interface INatsSubscriber
    {
        void Subscribe<T>(string subject, Func<T, Task> onMessage);
    }
}
