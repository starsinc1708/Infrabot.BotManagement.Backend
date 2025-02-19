using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Infrabot.BotManagement.Domain.DTOs;
using Infrabot.BotManagement.Domain.Grpc;
using Infrabot.BotManagement.Domain.Repositories;

namespace Infrabot.BotManagement.WebAPI.GrpcServices;

public class TelegramBotServiceImpl(TelegramBotRepository repository) 
    : TelegramBotService.TelegramBotServiceBase
{
    public override async Task<TelegramBotListResponse> GetAll(Empty request, ServerCallContext context)
    {
        var botList = await repository.GetAll(context.CancellationToken);
        var response = new TelegramBotListResponse();
        
        if (botList.Count == 0) return response;
        
        response.Bots.AddRange(botList.Select(MapBot));
        return response;
    }

    public override async Task<TelegramBotResponse> GetById(BotIdRequest request, ServerCallContext context)
    {
        var bot = await repository.GetById(request.BotId, context.CancellationToken);
        return bot != null ? new TelegramBotResponse { Bot = MapBot(bot) } : new TelegramBotResponse();
    }

    public override async Task<TelegramBotResponse> Add(CreateBotRequest request, ServerCallContext context)
    {
        var tgBotModel = new Domain.Models.TelegramBot
        {
            Id = request.Bot.Id,
            BotToken = request.Bot.Token,
            FirstName = request.Bot.FirstName,
            LastName = request.Bot.LastName,
            HasMainWebApp = request.Bot.HasMainWebApp,
            CanJoinGroups = request.Bot.CanJoinGroups,
            CanReadAllGroupMessages = request.Bot.CanReadAllGroupMessages,
            CanConnectToBusiness = request.Bot.CanConnectToBusiness,
            Enabled = request.Bot.Enabled
        };
        var bot = await repository.Add(tgBotModel, context.CancellationToken);
        return bot != null ? new TelegramBotResponse { Bot = MapBot(bot) } : new TelegramBotResponse();
    }
    
    public override async Task<TelegramBotResponse> Update(CreateBotRequest request, ServerCallContext context)
    {
        var existingBot = await repository.GetById(request.Bot.Id, context.CancellationToken);
        if (existingBot == null) return new TelegramBotResponse();

        existingBot.BotToken = request.Bot.Token;
        existingBot.FirstName = request.Bot.FirstName;
        existingBot.LastName = request.Bot.LastName;
        existingBot.CanJoinGroups = request.Bot.CanJoinGroups;
        existingBot.CanReadAllGroupMessages = request.Bot.CanReadAllGroupMessages;
        existingBot.SupportsInlineQueries = request.Bot.SupportsInlineQueries;
        existingBot.CanConnectToBusiness = request.Bot.CanConnectToBusiness;
        existingBot.HasMainWebApp = request.Bot.HasMainWebApp;
        existingBot.Enabled = request.Bot.Enabled;

        var updatedBot = await repository.Update(existingBot, context.CancellationToken);
        return updatedBot != null ? new TelegramBotResponse { Bot = MapBot(updatedBot) } : new TelegramBotResponse();
    }
    
    public override async Task<TelegramBotResponse> Patch(UpdateBotRequest request, ServerCallContext context)
    {
        var existingBot = await repository.GetById(request.Id, context.CancellationToken);
        if (existingBot == null) return new TelegramBotResponse();

        if (request.HasToken && !string.IsNullOrEmpty(request.Token)) existingBot.BotToken = request.Token;
        if (request.HasFirstName && !string.IsNullOrEmpty(request.FirstName)) existingBot.FirstName = request.FirstName;
        if (request.HasLastName && !string.IsNullOrEmpty(request.LastName)) existingBot.LastName = request.LastName;
        
        if (request.HasCanJoinGroups) existingBot.CanJoinGroups = request.CanJoinGroups;
        if (request.HasCanReadAllGroupMessages) existingBot.CanReadAllGroupMessages = request.CanReadAllGroupMessages;
        if (request.HasSupportsInlineQueries) existingBot.SupportsInlineQueries = request.SupportsInlineQueries;
        if (request.HasCanConnectToBusiness) existingBot.CanConnectToBusiness = request.CanConnectToBusiness;
        if (request.HasHasMainWebApp) existingBot.HasMainWebApp = request.HasMainWebApp;
        if (request.HasEnabled) existingBot.Enabled = request.Enabled;

        var updatedBot = await repository.Update(existingBot, context.CancellationToken);
        return updatedBot != null ? new TelegramBotResponse { Bot = MapBot(updatedBot) } : new TelegramBotResponse();
    }
    
    public override async Task<DeleteBotResponse> Delete(BotIdRequest request, ServerCallContext context)
    {
        var bot = await repository.GetById(request.BotId, context.CancellationToken);
        if (bot == null) return new DeleteBotResponse() { Success = false };
    
        await repository.Remove(bot, context.CancellationToken);
        return new DeleteBotResponse { Success = true };
    }

    public override async Task<TelegramBotWithModulesResponse> GetWithModules(BotIdRequest request, ServerCallContext context)
    {
       var bot = await repository.GetWithModules(request.BotId, context.CancellationToken);
       return bot != null ? CreateResponse(bot) : new TelegramBotWithModulesResponse();
    }

    public override async Task<TelegramBotWithModulesResponse> AddModules(BotModuleOperationRequest request, ServerCallContext context)
    {
        var bot = await repository.AddModules(request.BotId, new BotModuleDto.ModuleBotOperationDto
        {
            ModuleIds = request.ModuleIds.ToList()
        }, context.CancellationToken);

        return bot != null ? CreateResponse(bot) : new TelegramBotWithModulesResponse();
    }

    public override async Task<TelegramBotWithModulesResponse> RemoveModules(BotModuleOperationRequest request, ServerCallContext context)
    {
        var bot = await repository.RemoveModules(request.BotId, new BotModuleDto.ModuleBotOperationDto
        {
            ModuleIds = request.ModuleIds.ToList()
        }, context.CancellationToken);

        return bot != null ? CreateResponse(bot) : new TelegramBotWithModulesResponse();
    }

    private static TelegramBotWithModulesResponse CreateResponse(Domain.Models.TelegramBot bot)
    {
        var response = new TelegramBotWithModulesResponse
        {
            Bot = MapBot(bot)
        };

        response.BotModules.AddRange(bot.Modules.Select(module => new ProcessingModule
        {
            Id = module.Module.Id,
            HealthCheckEndpoint = module.Module.HealthCheckEndpoint,
            IpAddress = module.Module.IpAddress,
            Name = module.Module.Name,
            Port = module.Module.Port
        }));

        return response;
    }
    
    private static TelegramBot MapBot(Domain.Models.TelegramBot bot) => new()
    {
        Id = bot.Id,
        Token = bot.BotToken,
        FirstName = bot.FirstName ?? string.Empty,
        LastName = bot.LastName ?? string.Empty,
        CanJoinGroups = bot.CanJoinGroups,
        CanReadAllGroupMessages = bot.CanReadAllGroupMessages,
        SupportsInlineQueries = bot.SupportsInlineQueries,
        CanConnectToBusiness = bot.CanConnectToBusiness,
        HasMainWebApp = bot.HasMainWebApp,
        Enabled = bot.Enabled
    };
}
