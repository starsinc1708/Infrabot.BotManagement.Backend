using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace Infrabot.BotManagement.Domain;

public static class Extensions
{
    public static void AddBotManagementRedis(this WebApplicationBuilder builder, string redisConnectionString)
    {
        builder.Services.AddStackExchangeRedisCache(options => {
            options.Configuration = redisConnectionString;
            options.InstanceName = "Infrabot_BM_";
        });
    }
    
    public static void ConfigurePostgreDatabase<TDbC>(this WebApplicationBuilder builder, string dbConnectionString)
        where TDbC : DbContext
    {
        builder.Services.AddDbContext<TDbC>(opt =>
        {
            opt.UseNpgsql(dbConnectionString,
                o => o.MigrationsAssembly(typeof(TDbC).Assembly.FullName));
        });
    }
    
    public static async Task TryApplyDatabaseMigrations(this WebApplication webApplication)
    {
        await using var scope = webApplication.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        
        var context = services.GetRequiredService<BotManagementDbContext>();
        await context.Database.MigrateAsync();
        await context.InitializeDatabase();
    }
}