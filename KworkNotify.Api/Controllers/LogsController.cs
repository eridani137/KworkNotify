using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KworkNotify.Api.Controllers;

[Authorize]
[Route("logs")]
[ApiController]
public class LogsController : ControllerBase
{
    private const int TailLinesCount = 100;
    private readonly string _logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");

    [HttpGet("all")]
    public Task<IActionResult> DownloadAllLogs() => HandleLogRequest(false, false);

    [HttpGet("errors")]
    public Task<IActionResult> DownloadErrorLogs() => HandleLogRequest(true, false);

    [HttpGet("tail")]
    public Task<IActionResult> GetAllLogsTailText() => HandleLogRequest(false, true);

    [HttpGet("tail/errors")]
    public Task<IActionResult> GetErrorLogsTailText() => HandleLogRequest(true, true);

    private async Task<IActionResult> HandleLogRequest(bool errorsOnly, bool tailOnly)
    {
        try
        {
            var datePart = DateTime.Today.ToString("yyyyMMdd");
            var fileNamePrefix = errorsOnly ? "errors-" : "";
            var logFilePath = Path.Combine(_logsPath, $"{fileNamePrefix}{datePart}.log");
            var downloadFileName = $"logs-{fileNamePrefix}{datePart}{(tailOnly ? "-tail" : "")}.log";

            if (!System.IO.File.Exists(logFilePath))
            {
                return NotFound($"Файл логов {(errorsOnly ? "ошибок " : "")}за {DateTime.Today:yyyy-MM-dd} не найден");
            }

            if (tailOnly)
            {
                var lines = await ReadLastLines(logFilePath, TailLinesCount);
                var content = string.Join(Environment.NewLine, lines.Reverse());
                return Content(content, "text/plain");
            }

            await using FileStream fileStream = new(logFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fileStream, "text/plain", downloadFileName);
        }
        catch (Exception e)
        {
            const string error = "error retrieving logs";
            Log.ForContext<LogsController>().Error(e, error);
            return StatusCode(500, error);
        }
    }

    private static async Task<string[]> ReadLastLines(string filePath, int lineCount)
    {
        await using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader reader = new(stream);
        Queue<string> queue = new(lineCount);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (queue.Count >= lineCount)
            {
                queue.Dequeue();
            }
            queue.Enqueue(line);
        }

        return queue.ToArray();
    }
}