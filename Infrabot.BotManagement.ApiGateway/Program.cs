using System.Net;
using System.Text.Json.Serialization;
using Infrabot.BotManagement.ApiGateway;
using Infrabot.BotManagement.ApiGateway.Endpoints;
using Infrabot.BotManagement.Broker.Kafka;
using Infrabot.BotManagement.Domain;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 5002); // HTTP
    options.Listen(IPAddress.Any, 6002, listenOptions =>
    {
        listenOptions.UseHttps("/etc/ssl/certs/infrabot.ru.crt", "/etc/ssl/certs/infrabot.ru.key");
    });
});


builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi();
builder.AddGrpcClients();
builder.AddTelegramBotClient();
builder.Services.AddSingleton<KafkaProducer>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapTelegramApiEndpoints();
app.MapTelegramUpdatesEndpoints();
app.MapOpenApi();
app.MapScalarApiReference();

app.Run();