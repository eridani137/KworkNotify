using CliWrap;
using CliWrap.Buffered;
using KworkNotify.Core.Telegram;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace KworkNotify.Core.Service;

public class BackupManager(TelegramData data, IOptions<AppSettings> settings)
{
    public async Task<(bool Success, string Output, string Error)> CreateBackupAsync()
    {
        try
        {
            var scriptPath = settings.Value.BackupScriptPath;
            Log.ForContext<BackupManager>().Information("Starting backup script: {ScriptPath}", scriptPath);
            var result = await Cli.Wrap("bash")
                .WithArguments(scriptPath)
                .WithWorkingDirectory(settings.Value.BackupWorkingDirectory)
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();
            
            var stdout = result.StandardOutput;
            var stderr = result.StandardError;

            if (!string.IsNullOrEmpty(stderr) || result.ExitCode != 0)
            {
                Log.ForContext<BackupManager>().Error("Backup script failed. Output: {Output}, Error: {Error}, ExitCode: {ExitCode}",
                    stdout, stderr, result.ExitCode);
                return (false, stdout, stderr);
            }

            Log.ForContext<BackupManager>().Information("Backup created successfully");
            return (true, stdout, string.Empty);
        }
        catch (Exception e)
        {
            Log.ForContext<BackupManager>().Error(e, "Failed to create backup");
            return (false, string.Empty, e.Message);
        }
    }
    
    public async Task<(bool Success, List<string> SentFiles, string Error)> SendBackupsAsync()
        {
            try
            {
                if (data.Bot is not { } dataBot || dataBot.Client.TelegramClient is not { } telegramClient)
                {
                    const string error = "Telegram bot is not initialized";
                    Log.ForContext<BackupManager>().Warning(error);
                    return (false, [], error);
                }
                
                if (settings.Value.AdminIds.Count == 0)
                {
                    const string error = "Admin IDs are required";
                    Log.ForContext<BackupManager>().Error(error);
                    return (false, [], error);
                }

                var backupFiles = Directory.EnumerateFiles(settings.Value.BackupWorkingDirectory, "*.gz").ToList();

                if (backupFiles.Count == 0)
                {
                    const string error = "No backup files found";
                    Log.ForContext<BackupManager>().Error(error);
                    return (false, [], error);
                }

                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
                var time = DateTime.UtcNow + timeZone.BaseUtcOffset;
                var caption = $"Backup at {time:HH:mm:ss}";
                var mainAdminId = settings.Value.AdminIds.First();
                var sentFiles = new List<string>();

                await Task.WhenAll(backupFiles.Select(async backupFile =>
                {
                    try
                    {
                        await using var stream = new FileStream(backupFile, FileMode.Open, FileAccess.Read);
                        var input = new InputFileStream(stream, Path.GetFileName(backupFile));
                        await telegramClient.SendDocumentAsync(
                            new ChatId(mainAdminId),
                            document: input,
                            caption: caption);

                        System.IO.File.Delete(backupFile);
                        lock (sentFiles)
                        {
                            sentFiles.Add(backupFile);
                        }
                        Log.ForContext<BackupManager>().Information("Backup file sent and deleted: {BackupFile}", backupFile);
                    }
                    catch (Exception ex)
                    {
                        Log.ForContext<BackupManager>().Error(ex, "Failed to send backup file: {BackupFile}", backupFile);
                    }
                }));

                return (true, sentFiles, string.Empty);
            }
            catch (Exception ex)
            {
                Log.ForContext<BackupManager>().Error(ex, "Failed to send backups");
                return (false, [], ex.Message);
            }
        }
}