using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KworkNotify.Api.Controllers;

[Authorize]
[Route("logs")]
[ApiController]
public class LogsController : ControllerBase
{
    [HttpGet("all")]
    public async Task<IActionResult> DownloadAllLogs()
    {
        try
        {
            var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            var datePart = DateTime.Today.ToString("yyyyMMdd");
            var logFilePath = Path.Combine(logsPath, $"{datePart}.log");

            if (!System.IO.File.Exists(logFilePath))
            {
                return NotFound($"The log file for {DateTime.Today:yyyy-MM-dd} was not found");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(logFilePath);
            var downloadFileName = $"logs-{datePart}.log";

            return File(fileBytes, "text/plain", downloadFileName);
        }
        catch (Exception e)
        {
            const string error = "error retrieving logs";
            Log.ForContext<LogsController>().Error(e, error);
            return StatusCode(500, error);
        }
    }
    
    [HttpGet("errors")]
    public async Task<IActionResult> DownloadErrorLogs()
    {
        try
        {
            var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            var datePart = DateTime.Today.ToString("yyyyMMdd");
            var logFilePath = Path.Combine(logsPath, $"errors-{datePart}.log");

            if (!System.IO.File.Exists(logFilePath))
            {
                return NotFound($"The log file for {DateTime.Today:yyyy-MM-dd} was not found");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(logFilePath);
            var downloadFileName = $"logs-errors-{datePart}.log";

            return File(fileBytes, "text/plain", downloadFileName);
        }
        catch (Exception e)
        {
            const string error = "error retrieving errors logs";
            Log.ForContext<LogsController>().Error(e, error);
            return StatusCode(500, error);
        }
    }
}