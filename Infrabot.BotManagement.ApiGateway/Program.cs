using System.Text.Json.Serialization;
using Infrabot.BotManagement.ApiGateway;
using Infrabot.BotManagement.ApiGateway.Endpoints;
using Infrabot.BotManagement.Broker.Kafka;
using Infrabot.BotManagement.Domain;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5002, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
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

// Регистрация эндпоинтов
app.MapTelegramApiEndpoints();
app.MapTelegramUpdatesEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();
    
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
}

app.Run();
