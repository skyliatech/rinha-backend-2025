using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentGateway.Common.MessageBroker.Contracts
{
    public record NatsMessage(string Subject, byte[] Payload);
}
