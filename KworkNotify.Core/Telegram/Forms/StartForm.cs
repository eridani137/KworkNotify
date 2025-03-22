using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Service;
using KworkNotify.Core.Service.Cache;
using KworkNotify.Core.Service.Database;
using KworkNotify.Core.Service.Types;
using MongoDB.Driver;
using Serilog;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace KworkNotify.Core.Telegram.Forms;

// ReSharper disable once ClassNeverInstantiated.Global
public class StartForm : AutoCleanForm, ITelegramForm
{
    public MessageResult LastMessage { get; private set; }
    public required IMongoContext Context { get; set; }
    public required IAppCache Cache { get; set; }
    public required IAppSettings AppSettings { get; set; }
    public TelegramUser User { get; set; }
    public bool IsInitialized { get; set; }

#pragma warning disable CS8618, CS9264
    public StartForm()
#pragma warning restore CS8618, CS9264
    {
        DeleteMode = EDeleteMode.OnEveryCall;
    }

    public override async Task Action(MessageResult message)
    {
        if (!IsInitialized) return;
        if (message.GetData<CallbackData>() is not { } callback) return;
        message.Handled = true;
        
        Log.ForContext<StartForm>().Information("[UserAction] [{Device}] click {CallbackValue}", Device.DeviceId, callback.Value);
        switch (callback.Value)
        {
            case "send_updates":
                User.SendUpdates = !User.SendUpdates;
                User.Actions++;
                await Cache.ReplaceIfExistsAsync(User.Id.ToKey(), User, keepTtl: true);
                await Context.Users.UpdateOneAsync(u => u.Id == User.Id,
                    Builders<TelegramUser>.Update
                        .Set(u => u.SendUpdates, User.SendUpdates)
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
        LastMessage = message;
        if (!IsInitialized) return;
        
        var form = new ButtonForm();
        
        form.AddButtonRow(new ButtonBase($"{User.SendUpdates.EnabledOrDisabledToEmoji()} получать новые кворки", new CallbackData("a", "send_updates").Serialize()));

        await Device.Send("Главное меню", form);
    }
}