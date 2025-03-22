using KworkNotify.Core.Interfaces;
using TelegramBotBase;

namespace KworkNotify.Core.Telegram;

public class TelegramData : ITelegramData
{
    public required string Token { get; init; }
    public BotBase? Bot { get; set; }
}