using Telegram.Bot.Types.Enums;

namespace Infrabot.BotManagement.Domain.DTOs.BotRequests;

public abstract class SendTextMessage
{
    public record Request
    {
        public required List<long> ChatIdList { get; set; }
        public string? Text { get; set; }
        public ParseMode ParseMode { get; set; }
    }
}