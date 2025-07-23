using PaymentGateway.Common.MessageBroker.Subscriber;
using PaymentGateway.Common.Repository;
using PaymentGatewayWork.Rest.Base;
using PaymentGatewayWork.Rest.DefaultPayment;
using PaymentGatewayWork.Rest.FallbackPayment;
using PaymentGatewayWork.Services;
using PaymentGatewayWork.Works;
using StackExchange.Redis;

string urlPaymentDefault = "http://payment-processor-default:8080";
string urlPaymentFallback = "http://payment-processor-fallback:8080";

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Error);


builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("redis:6379"));


builder.Services.AddHttpClient(nameof(DefaultProcessorHealthCheckApi), c => c.BaseAddress = new Uri(urlPaymentDefault));
builder.Services.AddHttpClient(nameof(FallbackProcessorHealthCheckApi), c => c.BaseAddress = new Uri(urlPaymentFallback));

builder.Services.AddSingleton<IProcessorHealthCheckApi, DefaultProcessorHealthCheckApi>();
builder.Services.AddSingleton<IProcessorHealthCheckApi, FallbackProcessorHealthCheckApi>();

builder.Services.AddHttpClient(nameof(DefaultPaymentProcessorApi), c => c.BaseAddress = new Uri(urlPaymentDefault));
builder.Services.AddHttpClient(nameof(FallbackPaymentProcessorApi),  c => c.BaseAddress = new Uri(urlPaymentFallback));

builder.Services.AddSingleton<IPaymentProcessorApi, DefaultPaymentProcessorApi>();
builder.Services.AddSingleton<IPaymentProcessorApi, FallbackPaymentProcessorApi>();
builder.Services.AddSingleton<IProcessorHealthService, RedisProcessorHealthService>();
builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();

builder.Services.AddSingleton<IPaymentGatewayWorkService, PaymentGatewayWorkService>();
builder.Services.AddSingleton<INatsSubscriber, NatsSubscriber>();

builder.Services.AddHostedService<PaymentWorkerService>();
builder.Services.AddHostedService<PaymentRetryWorkService>();
builder.Services.AddHostedService<ProcessorHealthCheckWorkService>();


var host = builder.Build();
host.Run();
