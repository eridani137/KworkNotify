using KworkNotify.Core.Service;
using MongoDB.Driver;
using Serilog;
using Telegram.Bot.Types;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace KworkNotify.Core.Telegram.Forms;

// ReSharper disable once ClassNeverInstantiated.Global
public class StartForm : AutoCleanForm
{
    private TelegramUser? _user;
    private readonly MongoContext _context;
    private readonly AppSettings _settings;
    
    public StartForm(MongoContext context, AppSettings settings)
    {
        _context = context;
        _settings = settings;
        DeleteMode = EDeleteMode.OnEveryCall;
        Opened += OnOpened;
    }
    
    private async Task OnOpened(object sender, EventArgs e)
    {
        var role = TelegramRole.User;
        if (_settings.AdminIds.Contains(Device.DeviceId))
        {
            role = TelegramRole.Admin;
        }
        _user = await _context.GetOrAddUser(Device.DeviceId, role);
        if (_user is null) return;
        
        Log.ForContext<StartForm>().Information("{Command} [{Device}]", "/start", Device.DeviceId);
    }

    public override async Task Action(MessageResult message)
    {
        if (_user is null) return;
        if (message.GetData<CallbackData>() is not { } callback) return ;
        message.Handled = true;

        Log.ForContext<StartForm>().Information("[UserAction] [{Device}] click {CallbackValue}", Device.DeviceId, callback.Value);
        switch (callback.Value)
        {
            case "send_updates":
                _user.SendUpdates = !_user.SendUpdates;
                await _context.Users.UpdateOneAsync(u => u.Id == _user.Id,
                    Builders<TelegramUser>.Update.Set(u => u.SendUpdates, _user.SendUpdates));
                break;
            default:
                message.Handled = false;
                break;
        }
    }
    
    public override async Task Render(MessageResult message)
    {
        if (_user is null) return;
        
        var form = new ButtonForm();
        
        form.AddButtonRow(new ButtonBase($"{_user.SendUpdates.EnabledOrDisabledToEmoji()} получать новые кворки", new CallbackData("a", "send_updates").Serialize()));

        await Device.Send("Главное меню", form);
    }
}