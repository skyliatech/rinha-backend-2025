

namespace PaymentGateway.Common.MessageBroker.Subscriber
{
    public interface INatsSubscriber
    {
        /// <summary>
        /// Escuta mensagens no NATS de forma assíncrona usando o subject informado.
        /// Quando uma mensagem chegar, ela será desserializada para o tipo <typeparamref name="T"/> e enviada para o delegate <paramref name="onMessage"/>.
        /// As mensagens são processadas em paralelo (em múltiplas tasks) conforme chegam.
        /// </summary>
        void SubscribeAsync<T>(string subject, Func<T, Task> onMessage);

        /// <summary>
        /// Escuta mensagens no NATS de forma síncrona usando o subject informado.
        /// Fica em loop esperando mensagens, desserializa para o tipo <typeparamref name="T"/> e chama o delegate <paramref name="onMessage"/>.
        /// As mensagens são processadas uma por vez, em ordem de chegada. Pode ser cancelado via <paramref name="cancellationToken"/>.
        /// </summary>
        void SubscribeSync<T>(string subject, Action<T> onMessage, CancellationToken cancellationToken);
    }
}
