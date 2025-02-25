using System.Text;
using Infrabot.BotManagement.Domain.Grpc;

namespace Infrabot.BotManagement.ApiGateway;

public static class Extensions
{
    public static void AddGrpcClients(this WebApplicationBuilder builder)
    {
        var baseUrl = builder.Configuration[$"BotManagement:BaseUrl"];
        
        if (baseUrl == null) 
            throw new ArgumentNullException($"[{baseUrl}] - not found. Can't add grpc clients.");
        
        builder.Services.AddGrpcClient<UpdateSettingsService.UpdateSettingsServiceClient>(o =>
        {
            o.Address = new Uri(baseUrl);
        });

        builder.Services.AddGrpcClient<TgBotModuleService.TgBotModuleServiceClient>(o =>
        {
            o.Address = new Uri(baseUrl);
        });

        builder.Services.AddGrpcClient<ModuleUpdateSettingsService.ModuleUpdateSettingsServiceClient>(o =>
        {
            o.Address = new Uri(baseUrl);
            
        });
        
        builder.Services.AddGrpcClient<TelegramBotService.TelegramBotServiceClient>(o =>
        {
            o.Address = new Uri(baseUrl);
        });
    }
}