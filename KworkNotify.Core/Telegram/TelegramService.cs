using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Kwork;
using KworkNotify.Core.Service.Types;
using KworkNotify.Core.Telegram.Forms;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotBase;
using TelegramBotBase.Builder;
using TelegramBotBase.Commands;
using TelegramBotBase.States;

namespace KworkNotify.Core.Telegram;

public class TelegramService : IHostedService
{
    private readonly ITelegramData _data;
    private readonly IMongoContext _context;
    private readonly IAppCache _redis;
    private readonly IOptions<AppSettings> _settings;
    private readonly BotBase _bot;

    public TelegramService(ITelegramData data, IMongoContext context, KworkService kworkService, IAppCache redis, IOptions<AppSettings> settings)
    {
        _data = data;
        _context = context;
        _redis = redis;
        _settings = settings;
        
        var startFormFactory = new StartFormFactory()
        {
            Context = context,
            AppSettings = settings.Value,
            Cache = redis
        };

        _bot = BotBaseBuilder.Create()
            .WithAPIKey(data.Token)
            .DefaultMessageLoop()
            .WithStartFormFactory(startFormFactory)
            .NoProxy()
            .CustomCommands(c => { c.Add("start", "Запуск"); })
            .UseSerialization(new JsonStateMachine($@"{AppContext.BaseDirectory}\sessions.json"))
            .UseRussian()
            .UseThreadPool()
            .Build();

        _bot.SessionBegins += async (_, args) =>
        {
            if (args.Device.ActiveForm is StartForm { User: null } startForm)
            {
                if (await _context.GetOrAddUser(redis, settings.Value, args.DeviceId) is not { } user) return;
                startForm.User = user;
                startForm.IsInitialized = true;
                await startForm.Render(startForm.LastMessage);
            }
            Log.Information("New session [{Device}]", args.Device.DeviceId);
        };
        
        // _bot.BotCommand += async (_, args) =>
        // {
        //     switch (args.Command)
        //     {
        //     }
        // };

        data.Bot = _bot;

        kworkService.AddedNewProject += KworkServiceOnAddedNewProject;
    }
    private async Task KworkServiceOnAddedNewProject(object? sender, KworkProjectArgs e)
    {
        var project = await _context.Projects
            .Find(p => p.Id == e.KworkProject.Id)
            .FirstOrDefaultAsync();

        if (project == null)
        {
            // var usersToNotify = _data.Bot?.Sessions.SessionList.Values
            //     .Select(s => (Session: s, User: s.ActiveForm is ITelegramForm form ? form.User : null))
            //     .Where(x => x.User != null)
            //     .Select(x => (x.Session, x.User))
            //     .ToList();
            //
            // if (usersToNotify == null || usersToNotify.Count == 0)
            // {
            //     Log.ForContext<TelegramService>().Error("users not found");
            //     return;
            // }


            List<TelegramUser> usersToNotify;
            if (_settings.Value.IsDebug)
            {
                var mainId = _settings.Value.AdminIds.First();
                usersToNotify = await _context.Users
                    .Find(u => u.Id == mainId)
                    .ToListAsync();
            }
            else
            {
                usersToNotify = await _context.Users
                    .Find(u => u.SendUpdates)
                    .ToListAsync();
            }

            var projectText = e.KworkProject.ToString()
                .Replace("|SET_URL_HERE|", $"{_settings.Value.SiteUrl}/projects/{e.KworkProject.Id}/view");

            //foreach (var (session, user) in usersToNotify)
            foreach (var user in usersToNotify)
            {
                try
                {
                    Log.ForContext<TelegramService>().Information("[{Device}] send project '{ProjectName}'", user.Id, e.KworkProject.Name);
                    await _bot.Client.TelegramClient.SendTextMessageAsync(new ChatId(user.Id), projectText, disableWebPagePreview: true);
                    // var form = new ProjectForm()
                    // {
                    //     User = user,
                    //     Project = e.KworkProject
                    // };
                    // await session.ActiveForm.NavigateTo(form);
                    await _context.Users.UpdateOneAsync(u => u.Id == user.Id,
                        Builders<TelegramUser>.Update.Inc(u => u.ReceivedMessages, 1));
                }
                catch (Exception exception)
                {
                    Log.ForContext<TelegramService>().Error(exception, "The message was not sent to [{Device}]", user.Id);
                }
                finally
                {
                    await Task.Delay(500);
                }
            }

            if (!_settings.Value.IsDebug)
            {
                await _context.Projects.InsertOneAsync(e.KworkProject);
            }
        }
        else
        {
            await _context.Projects.UpdateOneAsync(
                p => p.Id == e.KworkProject.Id,
                Builders<KworkProject>.Update
                    .Set(p => p.GetWantsActiveCount, e.KworkProject.GetWantsActiveCount)
            );
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _bot.UploadBotCommands();
        await _bot.Start();
        // var users = await _context.Users.Find(u => true).ToListAsync(cancellationToken: cancellationToken);
        // var factory = new StartFormFactory()
        // {
        //     AppSettings = _settings.Value,
        //     Cache = _redis,
        //     Context = _context,
        // };
        // foreach (var session in _bot.Sessions.SessionList.Values.ToList())
        // {
        //     if (session.ActiveForm is not ITelegramForm telegramForm) continue;
        //     if (users.FirstOrDefault(u => u.Id == session.DeviceId) is not { } user)
        //     {
        //         _bot.Sessions.SessionList.Remove(session.DeviceId);
        //         continue;
        //     }
        //     telegramForm.AppSettings = _settings.Value;
        //     telegramForm.Cache = _redis;
        //     telegramForm.Context = _context;
        //     telegramForm.User = user;
        //     telegramForm.IsInitialized = true;
        //     session.ActiveForm.NavigationController = new NavigationController(factory.CreateForm());
        // }
        Log.ForContext<TelegramService>().Information("Telegram bot started");
        await _bot.Client.TelegramClient.SendTextMessageAsync(new ChatId(_settings.Value.AdminIds.First()), "Бот запущен", cancellationToken: cancellationToken);
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _bot.Stop();
        Log.ForContext<TelegramService>().Information("Telegram bot stopped");
        await _bot.Client.TelegramClient.SendTextMessageAsync(new ChatId(_settings.Value.AdminIds.First()), "Бот остановлен", cancellationToken: cancellationToken);
    }
}