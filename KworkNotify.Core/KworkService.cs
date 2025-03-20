using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace KworkNotify.Core;

public sealed class KworkService(KworkParser parser, IOptions<AppSettings> settings) : IHostedService
{
    private Task? _task;
    private CancellationTokenSource? _cts;
    private readonly Random _random = new();
    public event EventHandler<KworkProjectArgs>? OnAddedKworkProject;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Information("Kwork service started");
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _task = Worker();
        return Task.CompletedTask;
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information("Kwork service stopping");
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
            Log.Information("Kwork service stopped");
            _cts?.Dispose();
            _task?.Dispose();
        }
    }

    private async Task Worker()
    {
        if (_cts?.Token == null)
        {
            Log.Error("Kwork service can not be started");
            return;
        }
        Log.Information("Kwork service starting");
        await Task.Delay(3000, _cts.Token);
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                var update = parser.GetUpdate();
                await foreach (var project in update)
                {
                    
                    OnAddedProjectHandle(new KworkProjectArgs(project));
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception while executing Kwork service");
            }
            finally
            {
                var delay = _random.Next((int)TimeSpan.FromMinutes(settings.Value.MinDelay).TotalMilliseconds, (int)TimeSpan.FromMinutes(settings.Value.MaxDelay).TotalMilliseconds);
                Log.Information("Delay {S} minutes", TimeSpan.FromMilliseconds(delay).TotalMinutes.ToString("F1"));
                Log.Information("Next update {S}", DateTime.Now.AddMilliseconds(delay).ToString("HH:mm:ss"));
                await Task.Delay(delay, _cts.Token);
            }
        }
    }
    private void OnAddedProjectHandle(KworkProjectArgs e)
    {
        OnAddedKworkProject?.Invoke(this, e);
    }
}