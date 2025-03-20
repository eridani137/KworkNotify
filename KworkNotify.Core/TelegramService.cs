using KworkNotify.Core.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotBase;
using TelegramBotBase.Builder;
using TelegramBotBase.Commands;

namespace KworkNotify.Core;

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
        if (await _context.Projects.Find(p => p.ProjectId == e.Project.ProjectId).FirstOrDefaultAsync() is null)
        {
            await _context.Projects.InsertOneAsync(e.Project);
            var users = await _context.Users.Find(_ => true).ToListAsync();
            var projectText = e.Project.ToString().Replace("|SET_URL_HERE|", $"{_settings.Value.SiteUrl}/projects/{e.Project.ProjectId}/view");
            foreach (var user in users)
            {
                await _bot.Client.TelegramClient.SendTextMessageAsync(new ChatId(user.Id), projectText);
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _bot.UploadBotCommands();
        await _bot.Start();
        Log.Information("Telegram bot started");
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _bot.Stop();
        Log.Information("Telegram bot stopped");
    }
}