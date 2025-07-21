
namespace PaymentGatewayWork.Rest
{
    public class ServiceHealthResponse
    {
        public bool Failing { get; set; }
        public int MinResponseTime { get; set; }
    }
}
