using System.Text.Json.Serialization;
using Infrabot.BotManagement.ApiGateway;
using Infrabot.BotManagement.ApiGateway.Endpoints;
using Infrabot.BotManagement.Broker.Kafka;
using Infrabot.BotManagement.Domain;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Добавляем поддержку Forwarded Headers (для работы за прокси)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddOpenApi();
builder.AddGrpcClients();
builder.AddTelegramBotClient();
builder.Services.AddSingleton<KafkaProducer>();

var app = builder.Build();

app.UseForwardedHeaders();


// Настраиваем маршруты
app.MapTelegramApiEndpoints();
app.MapTelegramUpdatesEndpoints();
app.MapOpenApi();
app.MapScalarApiReference();

// Исправленный редирект, чтобы он учитывал прокси
app.MapGet("/", (HttpContext ctx) =>
{
    var prefix = ctx.Request.Headers["X-Forwarded-Prefix"].FirstOrDefault() ?? "";
    return Results.Redirect($"{prefix}/scalar");
});

// Запуск с явным указанием порта
app.Run("https://0.0.0.0:5002");