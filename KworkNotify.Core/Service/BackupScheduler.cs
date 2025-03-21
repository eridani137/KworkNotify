using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace KworkNotify.Core.Service;

public class BackupScheduler(BackupManager backupManager, IOptions<AppSettings> settings) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(settings.Value.BackupInterval);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.ForContext<BackupScheduler>().Information("Backup scheduler started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                Log.ForContext<BackupScheduler>().Error(ex, "Unexpected error in backup scheduler");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        Log.ForContext<BackupScheduler>().Information("Backup scheduler stopped");
    }
}