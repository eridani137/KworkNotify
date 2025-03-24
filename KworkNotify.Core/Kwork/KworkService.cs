using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Service.Types;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace KworkNotify.Core.Kwork;

public sealed class KworkService(IKworkParser parser, IAppCache redis, IOptions<AppSettings> settings) : IHostedService, IKworkService
{
    private Task? _task;
    private CancellationTokenSource? _cts;
    private readonly Random _random = new();
    public event Func<object?, KworkProjectArgs, Task>? AddedNewProject;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.ForContext<KworkService>().Information("Kwork service started");
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _task = Worker();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_cts != null)
            {
                await _cts.CancelAsync();
            }
            if (_task != null)
            {
                await Task.WhenAny(_task, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }
        finally
        {
            Log.ForContext<KworkService>().Information("Kwork service stopped");
            _cts?.Dispose();
            _task?.Dispose();
        }
    }

    public async Task Worker()
    {
        if (_cts?.Token == null)
        {
            Log.ForContext<KworkService>().Error("Kwork service can not be started");
            return;
        }
        await Task.Delay(3000, _cts.Token);
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                var update = parser.GetUpdate();
                await foreach (var project in update)
                {
                    if (_cts.Token.IsCancellationRequested) break;
                    if (await redis.KeyExistsAsync(project.GetKey))
                    {
                        if (AddedNewProject != null) continue;
                    }
                    await OnAddedNewProject(new KworkProjectArgs(project));
                }
            }
            catch (Exception e)
            {
                Log.ForContext<KworkService>().Error(e, "Exception while executing Kwork service");
            }
            finally
            {
                if (!_cts.IsCancellationRequested)
                {
                    var delay = _random.Next((int)TimeSpan.FromMinutes(settings.Value.MinDelay).TotalMilliseconds, (int)TimeSpan.FromMinutes(settings.Value.MaxDelay).TotalMilliseconds);
                    Log.ForContext<KworkService>().Information("Delay {S} minutes", TimeSpan.FromMilliseconds(delay).TotalMinutes.ToString("F1"));
                    Log.ForContext<KworkService>().Information("Next update {S}", DateTime.Now.AddMilliseconds(delay).ToString("HH:mm:ss"));
                    await Task.Delay(delay, _cts.Token);
                }
            }
        }
    }
    public async Task OnAddedNewProject(KworkProjectArgs args)
    {
        if (AddedNewProject != null)
        {
            await redis.SetKeyAsync(args.KworkProject.GetKey, TimeSpan.FromMinutes(15));

            var handlers = AddedNewProject.GetInvocationList()
                .Cast<Func<object?, KworkProjectArgs, Task>>();

            var tasks = handlers.Select(handler => handler.Invoke(this, args));
            await Task.WhenAll(tasks);
        }
        else
        {
            Log.ForContext<KworkService>().Information("OnAddedNewProject: {Name}", args.KworkProject.Name);
        }
    }
}