using KworkNotify.Core.Telegram;

namespace KworkNotify.Core.Interfaces;

public interface ITelegramForm
{
    IMongoContext Context { get; set; }
    IAppCache Cache { get; set; }
    IAppSettings AppSettings { get; set; }
    TelegramUser User { get; set; }
    bool IsInitialized { get; set; }
}