using System.Text.Json.Serialization;
using Telegram.Bot.Serialization;

namespace Infrabot.BotManagement.Domain.Enums;

[JsonConverter(typeof(EnumConverter<UpdateSource>))]
public enum UpdateSource
{
    PrivateChat,
    Channel,
    Group,
    SuperGroup,
    BusinessAccount,
    InlineMode,
    Payment,
    Poll,
    Unknown,
    Test
}