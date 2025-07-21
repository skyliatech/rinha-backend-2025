using PaymentGateway.Common.MessageBroker.Publisher;
using PaymentGateway.Common.Repository;
using PaymentGatewayApi.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment - Rinha-backend - 2025", Version = "v1" });

    // Opcional: Configuração de segurança (JWT Bearer Token, por exemplo)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o JWT com Bearer no campo. Exemplo: 'Bearer seuTokenAqui'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<INatsPublisher, NatsPublisher>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseReDoc(c =>
{
    c.DocumentTitle = "Payment - Rinha-backend - 2025";
    c.SpecUrl = "/swagger/v1/swagger.json";
    c.RoutePrefix = "docs"; 
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();