using TelegramBotBase;

namespace KworkNotify.Core.Telegram;

public class TelegramData
{
    public required string Token { get; init; }
    public BotBase? Bot { get; set; }
}