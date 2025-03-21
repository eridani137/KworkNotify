using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KworkNotify.Api.Controllers;

[Authorize]
[Route("logs")]
[ApiController]
public class LogsController : ControllerBase
{
    private const int TailLinesCount = 250;
    private readonly string _logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
    
    [HttpGet("all")]
    public async Task<IActionResult> DownloadAllLogs()
    {
        return await HandleLogRequest(false, false);
    }

    [HttpGet("errors")]
    public async Task<IActionResult> DownloadErrorLogs()
    {
        return await HandleLogRequest(true, false);
    }

    [HttpGet("tail/all")]
    public async Task<IActionResult> GetAllLogsTail()
    {
        return await HandleLogRequest(false, true);
    }

    [HttpGet("tail/errors")]
    public async Task<IActionResult> GetErrorLogsTail()
    {
        return await HandleLogRequest(true, true);
    }
    
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
                var bytes = System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines));
                return File(bytes, "text/plain", downloadFileName);
            }
            
            var fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fileStream, "text/plain", downloadFileName);
        }
        catch (Exception e)
        {
            const string error = "error retrieving logs";
            Log.ForContext<LogsController>().Error(e, error);
            return StatusCode(500, error);
        }
    }
    
    private async Task<string[]> ReadLastLines(string filePath, int lineCount)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        var queue = new Queue<string>(lineCount);
            
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