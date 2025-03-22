using KworkNotify.Core.Interfaces;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace KworkNotify.Core.Telegram.Forms;

public class StartFormFactory : IStartFormFactory, ITelegramForm
{
    public required IMongoContext Context { get; set; }
    public required IAppCache Cache { get; set; }
    public required IAppSettings AppSettings { get; set; }
    public TelegramUser User { get; set; } = null!;
    public bool IsInitialized { get; set; }

    public FormBase CreateForm()
    {
        return new StartForm()
        {
            Context = Context,
            Cache = Cache,
            AppSettings = AppSettings
        };
    }
}