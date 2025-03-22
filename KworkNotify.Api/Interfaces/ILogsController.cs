using Microsoft.AspNetCore.Mvc;

namespace KworkNotify.Api.Interfaces;

public interface ILogsController
{
    Task<IActionResult> DownloadAllLogs();
    Task<IActionResult> DownloadErrorLogs();
    Task<IActionResult> GetAllLogsTailText();
    Task<IActionResult> GetErrorLogsTailText();
    Task<IActionResult> HandleLogRequest(bool errorsOnly, bool tailOnly);
}