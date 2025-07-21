using NATS.Client;
using PaymentGateway.Common.MessageBroker.Publisher;
using PaymentGatewayWork.Rest.Base;
using PaymentGatewayWork.Rest.DefaultPayment;
using PaymentGatewayWork.Rest.FallbackPayment;
using PaymentGatewayWork.Services;
using PaymentGatewayWork.Works;
using StackExchange.Redis;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("redis-rinha:6379"));

builder.Services.AddHttpClient<DefaultProcessorHealthCheckApi>(c => c.BaseAddress = new Uri("http://default-processor"));
builder.Services.AddHttpClient<FallbackProcessorHealthCheckApi>(c => c.BaseAddress = new Uri("http://fallback-processor"));

builder.Services.AddSingleton<IProcessorHealthCheckApi, DefaultProcessorHealthCheckApi>();
builder.Services.AddSingleton<IProcessorHealthCheckApi, FallbackProcessorHealthCheckApi>();

builder.Services.AddHttpClient<DefaultPaymentProcessorApi>(c => c.BaseAddress = new Uri("http://default-processor"));
builder.Services.AddHttpClient<FallbackPaymentProcessorApi>(c => c.BaseAddress = new Uri("http://fallback-processor"));

builder.Services.AddSingleton<IPaymentProcessorApi, DefaultPaymentProcessorApi>();
builder.Services.AddSingleton<IPaymentProcessorApi, FallbackPaymentProcessorApi>();
builder.Services.AddSingleton<IProcessorHealthService, RedisProcessorHealthService>();

builder.Services.AddSingleton<IPaymentGatewayWorkService, PaymentGatewayWorkService>();
builder.Services.AddSingleton<INatsPublisher, NatsPublisher>();

builder.Services.AddHostedService<PaymentWorkerService>();
builder.Services.AddHostedService<PaymentRetryWorkService>();
builder.Services.AddHostedService<ProcessorHealthCheckWorkService>();


var host = builder.Build();
host.Run();
