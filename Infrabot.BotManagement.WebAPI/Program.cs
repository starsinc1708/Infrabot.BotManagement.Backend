using Infrabot.BotManagement.Domain;
using Infrabot.BotManagement.Domain.Repositories;
using Infrabot.BotManagement.WebAPI.GrpcServices;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddOpenApi();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var dbConnectionString = builder.Configuration["PostgreSQL:BotManagementDb"] 
                         ?? throw new NullReferenceException("PostgreSQL:BotManagementDb");

Console.WriteLine($"dbConnectionString: {dbConnectionString}");

var redisConnectionString = builder.Configuration["ConnectionStrings:Redis"]
                            ?? throw new InvalidOperationException("Connection string for 'Redis' not found.");

Console.WriteLine($"redisConnectionString: {redisConnectionString}");

builder.ConfigurePostgreDatabase<BotManagementDbContext>(dbConnectionString);
builder.AddBotManagementRedis(redisConnectionString);

builder.Services.AddScoped<UpdateSettingsRepository>();
builder.Services.AddScoped<ModuleUpdateSettingsRepository>();
builder.Services.AddScoped<TelegramBotRepository>();
builder.Services.AddScoped<TgBotModuleRepository>();

var app = builder.Build();

app.MapGrpcService<ModuleUpdateSettingsServiceImpl>();
app.MapGrpcService<TelegramBotServiceImpl>();
app.MapGrpcService<TgBotModuleServiceImpl>();
app.MapGrpcService<UpdateSettingsServiceImpl>();

app.MapOpenApi();
app.MapGrpcReflectionService();

await app.TryApplyDatabaseMigrations();

app.Run();
