using Infrabot.BotManagement.Domain.Models;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;

namespace Infrabot.BotManagement.Domain.Mappings;

public static class TelegramBotMapping
{
    public static TelegramBot? MapToModel(this User botInfo, string token)
    {
        if (string.IsNullOrEmpty(token)) return null;

        return new TelegramBot()
        {
            FirstName = botInfo.FirstName,
            LastName = botInfo.LastName,
            BotToken = token,
            CanConnectToBusiness = botInfo.CanConnectToBusiness,
            CanJoinGroups = botInfo.CanJoinGroups,
            CanReadAllGroupMessages = botInfo.CanReadAllGroupMessages,
            HasMainWebApp = botInfo.HasMainWebApp,
            SupportsInlineQueries = botInfo.SupportsInlineQueries,
            Id = botInfo.Id
        };
    }
}