using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Service.Types;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace KworkNotify.Core.Service.Statistic;

public class UsageStatisticService(IAppCache redis, IOptions<AppSettings> settings) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(settings.Value.StatisticPushInterval);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.ForContext<UsageStatisticService>().Information("Statistic service is running");

        var lastPushTimeStr = await redis.GetAsync("statistic");
        var delayTime = TimeSpan.Zero;
        
        if (!string.IsNullOrEmpty(lastPushTimeStr) && DateTime.TryParse(lastPushTimeStr, out var lastBackupTime))
        {
            var nextBackupTime = lastBackupTime + _interval;
            delayTime = nextBackupTime - DateTime.UtcNow;

            if (delayTime <= TimeSpan.Zero)
            {
                delayTime = TimeSpan.Zero;
            }

            Log.ForContext<UsageStatisticService>().Information("Statistic push in {Delay} minutes", delayTime.ToString());
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(delayTime, stoppingToken);

                Log.ForContext<UsageStatisticService>().Information("Starting statistics push at {Time}", DateTime.UtcNow);
                
                
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                Log.ForContext<UsageStatisticService>().Error(e, "Error while executing service");
            }
        }
    }
}