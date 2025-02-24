using System.Text.Json.Serialization;
using Infrabot.BotManagement.ApiGateway;
using Infrabot.BotManagement.ApiGateway.Endpoints;
using Infrabot.BotManagement.Broker.Kafka;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi();

builder.AddGrpcClients();

builder.Services.AddSingleton<KafkaProducer>();

var app = builder.Build();

app.MapTelegramApiEndpoints();
app.MapTelegramUpdatesEndpoints();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

app.Run();