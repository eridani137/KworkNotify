using System.Text.Json;
using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Kwork;
using KworkNotify.Core.Service.Types;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace KworkNotify.Core.Service;

public class TelegramService : IHostedService
{
    private readonly IMongoContext _context;
    private readonly IOptions<AppSettings> _settings;
    private readonly ITelegramBotClient _bot;
    private readonly TelegramData _telegramData;

    public TelegramService(IMongoContext context, KworkService kworkService, IOptions<AppSettings> settings, ITelegramBotClient bot, TelegramData telegramData)
    {
        _context = context;
        _settings = settings;
        _bot = bot;
        _telegramData = telegramData;

        kworkService.AddedNewProject += KworkServiceOnAddedNewProject;
    }
    private async Task KworkServiceOnAddedNewProject(object? sender, KworkProjectArgs e)
    {
        var project = await _context.Projects
            .Find(p => p.Id == e.KworkProject.Id)
            .FirstOrDefaultAsync();

        if (project == null)
        {
            var projectText = e.KworkProject.ToString()
                .Replace("|SET_URL_HERE|", $"{_settings.Value.SiteUrl}/projects/{e.KworkProject.Id}/view");

            await _bot.SendMessage(_telegramData.ChannelId, projectText,
                linkPreviewOptions: new LinkPreviewOptions() { IsDisabled = true });
            
            // await _client.SendMessageAsync(_channel.ToInputPeer(), projectText, preview: Client.LinkPreview.Disabled);

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
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = {  }
        };
        _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
        
        var me = await _bot.GetMe(cancellationToken: cancellationToken);
        Log.Information("Start bot @{Username}", me.Username);
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        Log.ForContext<TelegramService>().Information("Telegram bot stopped");
        return Task.CompletedTask;
    }
    
    private Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var str = JsonSerializer.Serialize(exception);
        Log.ForContext<TelegramService>().Error("error: {Str}", str);
        return Task.CompletedTask;
    }
}