using Infrabot.BotManagement.Domain.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Infrabot.BotManagement.Domain.UpdateProcessingUtils;

public static class DistributeUpdateHelper
{
    public static UpdateSource DetermineUpdateSource(Update update)
    {
        return update.Type switch
        {
            UpdateType.BusinessConnection
                or UpdateType.BusinessMessage
                or UpdateType.DeletedBusinessMessages
                or UpdateType.EditedBusinessMessage => UpdateSource.BusinessAccount,

            UpdateType.CallbackQuery => GetUpdateSourceFromChatType(update.CallbackQuery!.Message!.Chat.Type),

            UpdateType.ChannelPost
                or UpdateType.EditedChannelPost => UpdateSource.Channel,

            UpdateType.ChatBoost => GetUpdateSourceFromChatType(update.ChatBoost!.Chat.Type),
            UpdateType.RemovedChatBoost => GetUpdateSourceFromChatType(update.RemovedChatBoost!.Chat.Type),

            UpdateType.ChatMember => GetUpdateSourceFromChatType(update.ChatMember!.Chat.Type),
            UpdateType.ChatJoinRequest => GetUpdateSourceFromChatType(update.ChatJoinRequest!.Chat.Type),
            UpdateType.MyChatMember => GetUpdateSourceFromChatType(update.MyChatMember!.Chat.Type),

            UpdateType.ChosenInlineResult
                or UpdateType.InlineQuery => UpdateSource.InlineMode,

            UpdateType.Message => GetUpdateSourceFromChatType(update.Message!.Chat.Type),
            UpdateType.EditedMessage => GetUpdateSourceFromChatType(update.EditedMessage!.Chat.Type),

            UpdateType.MessageReaction => GetUpdateSourceFromChatType(update.MessageReaction!.Chat.Type),
            UpdateType.MessageReactionCount => GetUpdateSourceFromChatType(update.MessageReactionCount!.Chat.Type),

            UpdateType.Poll
                or UpdateType.PollAnswer => UpdateSource.Poll,

            UpdateType.PreCheckoutQuery
                or UpdateType.PurchasedPaidMedia
                or UpdateType.ShippingQuery => UpdateSource.Payment,

            UpdateType.Unknown => UpdateSource.Unknown,
            _ => UpdateSource.Unknown
        };
    }
    
    private static UpdateSource GetUpdateSourceFromChatType(ChatType chatType)
    {
        return chatType switch
        {
            ChatType.Group => UpdateSource.Group,
            ChatType.Supergroup => UpdateSource.SuperGroup,
            ChatType.Private => UpdateSource.PrivateChat,
            ChatType.Channel => UpdateSource.Channel,
            _ => default
        };
    }
}