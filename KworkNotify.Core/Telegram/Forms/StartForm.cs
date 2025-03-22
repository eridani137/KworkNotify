using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Service;
using KworkNotify.Core.Service.Cache;
using KworkNotify.Core.Service.Database;
using KworkNotify.Core.Service.Types;
using MongoDB.Driver;
using Serilog;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace KworkNotify.Core.Telegram.Forms;

// ReSharper disable once ClassNeverInstantiated.Global
public class StartForm : AutoCleanForm
{
    private TelegramUser? _user;
    private readonly IMongoContext _context;
    private readonly IAppCache _redis;
    private readonly IAppSettings _settings;
    
    public StartForm(IMongoContext context, IAppCache redis, IAppSettings settings)
    {
        _context = context;
        _redis = redis;
        _settings = settings;
        DeleteMode = EDeleteMode.OnEveryCall;
        Opened += OnOpened;
    }
    
    private async Task OnOpened(object sender, EventArgs e)
    {
        _user = await _context.OnOpenedForm(_redis, _settings, Device.DeviceId);
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
                _user.Actions++;
                await _redis.ReplaceIfExistsAsync(_user.Id.ToKey(), _user, keepTtl: true);
                await _context.Users.UpdateOneAsync(u => u.Id == _user.Id,
                    Builders<TelegramUser>.Update
                        .Set(u => u.SendUpdates, _user.SendUpdates)
                        .Inc(u => u.Actions, 1)
                    );
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