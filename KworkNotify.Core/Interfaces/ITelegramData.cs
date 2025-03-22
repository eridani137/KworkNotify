using TelegramBotBase;

namespace KworkNotify.Core.Interfaces;

public interface ITelegramData
{
    string Token { get; init; }
    BotBase? Bot { get; set; }
}