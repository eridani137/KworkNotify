namespace KworkNotify.Core.Interfaces;

public interface IBackupManager
{
    Task<(bool Success, string Output, string Error)> CreateBackupAsync();
    Task<(bool Success, List<string> SentFiles, string Error)> SendBackupsAsync();
}