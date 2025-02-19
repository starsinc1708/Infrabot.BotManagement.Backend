using Infrabot.BotManagement.Domain;
using Infrabot.BotManagement.Domain.Repositories;
using Infrabot.BotManagement.WebAPI.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

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

app.MapGrpcService<ModuleUpdateSettingsServiceImpl>();
app.MapGrpcService<TelegramBotServiceImpl>();
app.MapGrpcService<TgBotModuleServiceImpl>();
app.MapGrpcService<UpdateSettingsServiceImpl>();

/*if (app.Environment.IsDevelopment())
{
    
}*/

app.MapOpenApi();
app.MapGrpcReflectionService();

await app.TryApplyDatabaseMigrations();

app.Run();
