using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Kwork;
using KworkNotify.Core.Service.Types;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;
using TL;
using WTelegram;

namespace KworkNotify.Core.Service;

public class TelegramService : IHostedService
{
    private readonly Client _client;
    private readonly Channel _channel;
    private readonly IMongoContext _context;
    private readonly IOptions<AppSettings> _settings;

    public TelegramService(Client client, Channel channel, IMongoContext context, KworkService kworkService, IAppCache redis, IOptions<AppSettings> settings)
    {
        _client = client;
        _channel = channel;
        _context = context;
        _settings = settings;

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

            await _client.SendMessageAsync(_channel.ToInputPeer(), projectText, preview: Client.LinkPreview.Disabled);

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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.ForContext<TelegramService>().Information("Telegram bot started {Channel}", _channel?.title);
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        Log.ForContext<TelegramService>().Information("Telegram bot stopped");
        return Task.CompletedTask;
    }
}