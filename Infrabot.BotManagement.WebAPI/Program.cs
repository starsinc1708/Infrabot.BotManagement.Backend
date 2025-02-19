using Infrabot.BotManagement.Domain;
using Infrabot.BotManagement.Domain.Repositories;
using Infrabot.BotManagement.WebAPI.GrpcServices;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var dbConnectionString = builder.Configuration["PostgreSQL:BotManagementDb"] 
                         ?? throw new NullReferenceException("PostgreSQL:BotManagementDb");
var redisConnectionString = builder.Configuration["ConnectionStrings:Redis"]
                            ?? throw new InvalidOperationException("Connection string for 'Redis' not found.");

builder.ConfigurePostgreDatabase<BotManagementDbContext>(dbConnectionString);
builder.AddBotManagementRedis(redisConnectionString);

builder.Services.AddScoped<UpdateSettingsRepository>();
builder.Services.AddScoped<ModuleUpdateSettingsRepository>();
builder.Services.AddScoped<TelegramBotRepository>();
builder.Services.AddScoped<TgBotModuleRepository>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
});

app.Use((context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-Forwarded-Prefix", out var prefix))
    {
        context.Request.PathBase = new PathString(prefix);
    }
    return next();
});

app.MapGrpcService<ModuleUpdateSettingsServiceImpl>();
app.MapGrpcService<TelegramBotServiceImpl>();
app.MapGrpcService<TgBotModuleServiceImpl>();
app.MapGrpcService<UpdateSettingsServiceImpl>();

app.MapOpenApi();
app.MapGrpcReflectionService();

await app.TryApplyDatabaseMigrations();

app.Run();
