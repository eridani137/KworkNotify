using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace KworkNotify.Core.Service;

public class BackupScheduler(BackupManager backupManager, AppCache redis, IOptions<AppSettings> settings) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(settings.Value.BackupInterval);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.ForContext<BackupScheduler>().Information("Backup scheduler started");

        var lastBackupStr = await redis.GetAsync("backup");
        var delayTime = TimeSpan.Zero;

        if (!string.IsNullOrEmpty(lastBackupStr) && DateTime.TryParse(lastBackupStr, out var lastBackupTime))
        {
            var nextBackupTime = lastBackupTime + _interval;
            delayTime = nextBackupTime - DateTime.UtcNow;

            if (delayTime <= TimeSpan.Zero)
            {
                delayTime = TimeSpan.Zero;
            }

            Log.ForContext<BackupScheduler>().Information("Backup in {Delay} minutes", delayTime.ToString());
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(delayTime, stoppingToken);

                Log.ForContext<BackupScheduler>().Information("Starting backup process at {Time}", DateTime.UtcNow);

                var (createSuccess, createOutput, createError) = await backupManager.CreateBackupAsync();
                if (!createSuccess)
                {
                    Log.ForContext<BackupScheduler>().Error("Backup creation failed. Output: {Output}, Error: {Error}", createOutput, createError);
                }
                else
                {
                    Log.ForContext<BackupScheduler>().Information("Backup created successfully: {Output}", createOutput);
                }

                var (sendSuccess, sentFiles, sendError) = await backupManager.SendBackupsAsync();
                if (!sendSuccess)
                {
                    Log.ForContext<BackupScheduler>().Error("Backup sending failed. Error: {Error}", sendError);
                }
                else
                {
                    Log.ForContext<BackupScheduler>().Information("Backup sent successfully. Files: {Files}", string.Join(", ", sentFiles));
                }

                if (createSuccess && sendSuccess)
                {
                    await redis.SetAsync("backup", DateTime.UtcNow.ToString("O"), _interval);
                    delayTime = _interval;
                }
                else
                {
                    ReloadBackup(new Exception());
                }
            }
            catch (Exception e)
            {
                ReloadBackup(e);
            }

            
        }

        Log.ForContext<BackupScheduler>().Information("Backup scheduler stopped");
        return;
        
        void ReloadBackup(Exception e)
        {
            Log.ForContext<BackupScheduler>().Error(e, "Unexpected error in backup scheduler");
            delayTime = TimeSpan.FromMinutes(10);
        }
    }
}