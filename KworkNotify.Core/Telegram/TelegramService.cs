using System.Text;
using System.Text.RegularExpressions;
using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Kwork;
using KworkNotify.Core.Service.Types;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;
using TL;
using WTelegram;

namespace KworkNotify.Core.Telegram;

public class TelegramService : IHostedService
{
    // private readonly ITelegramData _data;
    private readonly Client _client;
    private readonly IMongoContext _context;
    private readonly IAppCache _redis;
    private readonly IOptions<AppSettings> _settings;
    // private readonly BotBase _bot;
    private Channel? Channel { get; set; }
    public DialogBase? Dialog { get; set; }

    public TelegramService(Client client, User user, IMongoContext context, KworkService kworkService, IAppCache redis, IOptions<AppSettings> settings)
    {
        _client = client;
        _context = context;
        _redis = redis;
        _settings = settings;

        kworkService.AddedNewProject += KworkServiceOnAddedNewProject;
    }
    private async Task KworkServiceOnAddedNewProject(object? sender, KworkProjectArgs e)
    {
        if (Channel == null) return;
        
        var project = await _context.Projects
            .Find(p => p.Id == e.KworkProject.Id)
            .FirstOrDefaultAsync();

        if (project == null)
        {
            // List<TelegramUser> usersToNotify;
            // if (_settings.Value.IsDebug)
            // {
            //     var mainId = _settings.Value.AdminIds.First();
            //     usersToNotify = await _context.Users
            //         .Find(u => u.Id == mainId)
            //         .ToListAsync();
            // }
            // else
            // {
            //     usersToNotify = await _context.Users
            //         .Find(u => u.SendUpdates)
            //         .ToListAsync();
            // }

            var projectText = e.KworkProject.ToString()
                .Replace("|SET_URL_HERE|", $"{_settings.Value.SiteUrl}/projects/{e.KworkProject.Id}/view");

            // foreach (var user in usersToNotify)
            // {
            //     try
            //     {
            //         Log.ForContext<TelegramService>().Information("[{Device}] send project '{ProjectName}'", user.Id, e.KworkProject.Name);
            //         // await _bot.Client.TelegramClient.SendTextMessageAsync(new ChatId(user.Id), projectText, disableWebPagePreview: true);
            //         await _context.Users.UpdateOneAsync(u => u.Id == user.Id,
            //             Builders<TelegramUser>.Update.Inc(u => u.ReceivedMessages, 1));
            //     }
            //     catch (Exception exception)
            //     {
            //         Log.ForContext<TelegramService>().Error(exception, "The message was not sent to [{Device}]", user.Id);
            //     }
            //     finally
            //     {
            //         await Task.Delay(500);
            //     }
            // }

            await _client.SendMessageAsync(Channel.ToInputPeer(), projectText, preview: Client.LinkPreview.Disabled);

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
        var chats = await _client.Messages_GetAllDialogs();
        Channel = chats.chats[2615890333] as Channel;
        foreach (var dialogBase in chats.dialogs)
        {
            if (dialogBase.Peer.ID != 1460469940) continue;
            Dialog = dialogBase;
            break;
        }
        // await _bot.UploadBotCommands();
        // await _bot.Start();
        Log.ForContext<TelegramService>().Information("Telegram bot started {Channel}", Channel?.title);
        // await _bot.Client.TelegramClient.SendTextMessageAsync(new ChatId(_settings.Value.AdminIds.First()), "Бот запущен", cancellationToken: cancellationToken);
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // await _bot.Stop();
        Log.ForContext<TelegramService>().Information("Telegram bot stopped");
        // await _bot.Client.TelegramClient.SendTextMessageAsync(new ChatId(_settings.Value.AdminIds.First()), "Бот остановлен", cancellationToken: cancellationToken);
    }
}