using CliWrap;
using CliWrap.Buffered;
using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Service.Types;
using Microsoft.Extensions.Options;
using Serilog;
using TL;
using WTelegram;

namespace KworkNotify.Core.Service.Backup;

public class BackupManager(Client client, IOptions<AppSettings> settings) : IBackupManager
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
            var sentFiles = new List<string>();

            await Task.WhenAll(backupFiles.Select(async backupFile =>
            {
                try
                {
                    var inputFile = await client.UploadFileAsync(backupFile);
                    await client.SendMediaAsync(InputPeer.Self, caption, inputFile);
                    
                    // var input = new InputFileStream(stream, Path.GetFileName(backupFile));
                    // await telegramClient.SendDocumentAsync(
                    //     new ChatId(mainAdminId),
                    //     document: input,
                    //     caption: caption); // TODO
                    
                    File.Delete(backupFile);
                    lock (sentFiles)
                    {
                        sentFiles.Add(backupFile);
                    }
                    Log.ForContext<BackupManager>().Information("Backup file sent and deleted: {BackupFile}", backupFile); // TODO
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