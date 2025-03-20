using KworkNotify.Core.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using TelegramBotBase;
using TelegramBotBase.Builder;
using TelegramBotBase.Commands;

namespace KworkNotify.Core;

public class TelegramService : IHostedService
{
    private readonly BotBase _bot;

    public TelegramService(TelegramToken token, MongoContext context, IOptions<AppSettings> settings)
    {
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