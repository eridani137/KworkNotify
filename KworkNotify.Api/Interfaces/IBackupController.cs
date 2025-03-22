using Microsoft.AspNetCore.Mvc;

namespace KworkNotify.Api.Interfaces;

public interface IBackupController
{
    Task<ActionResult> CreateBackup();
    Task<ActionResult<List<string>>> SendBackup();
}