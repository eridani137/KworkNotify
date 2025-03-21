using KworkNotify.Core.Kwork;
using KworkNotify.Core.Service;
using KworkNotify.Core.Telegram.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotBase;
using TelegramBotBase.Builder;
using TelegramBotBase.Commands;

namespace KworkNotify.Core.Telegram;

public class TelegramService : IHostedService
{
    private readonly MongoContext _context;
    private readonly IOptions<AppSettings> _settings;
    private readonly BotBase _bot;

    public TelegramService(TelegramToken token, MongoContext context, KworkService kworkService, IOptions<AppSettings> settings)
    {
        _context = context;
        _settings = settings;
        var serviceCollection = new ServiceCollection()
            .AddSingleton<MongoContext>(_ => context)
            .AddSingleton<AppSettings>(_ => settings.Value);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        _bot = BotBaseBuilder.Create()
            .WithAPIKey(token.Token)
            .DefaultMessageLoop()
            .WithServiceProvider<StartForm>(serviceProvider)
            .NoProxy()
            .CustomCommands(c => { c.Add("start", "Запуск"); })
            // .UseJSON()
            .NoSerialization()
            .UseRussian()
            .UseThreadPool()
            .Build();

        kworkService.AddedNewProject += KworkServiceOnAddedNewProject;

        // _bot.BotCommand += (_, args) =>
        // {
        //     const string startCommand = "/start";
        //
        //     switch (args.Command)
        //     {
        //         case startCommand:
        //             Log.Information("{Command} on {Device}", startCommand, args.Device.DeviceId);
        //             break;
        //     }
        //     
        //     return Task.CompletedTask;
        // };
    }
    private async Task KworkServiceOnAddedNewProject(object? sender, KworkProjectArgs e)
    {
        if (await _context.Projects.Find(p => p.Id == e.KworkProject.Id).FirstOrDefaultAsync() is null)
        {
            await _context.Projects.InsertOneAsync(e.KworkProject);
            var users = await _context.Users.Find(u => u.SendUpdates).ToListAsync();
            var projectText = e.KworkProject.ToString().Replace("|SET_URL_HERE|", $"{_settings.Value.SiteUrl}/projects/{e.KworkProject.Id}/view");
            foreach (var user in users)
            {
                try
                {
                    Log.ForContext<TelegramService>().Information("[{Device}] send project '{ProjectName}'", user.Id, e.KworkProject.Name);
                    await _bot.Client.TelegramClient.SendTextMessageAsync(new ChatId(user.Id), projectText, disableWebPagePreview: true);
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
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _bot.UploadBotCommands();
        await _bot.Start(); 
        Log.ForContext<TelegramService>().Information("Telegram bot started");
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _bot.Stop();
        Log.ForContext<TelegramService>().Information("Telegram bot stopped");
    }
}