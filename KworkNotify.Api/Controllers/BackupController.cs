using CliWrap;
using CliWrap.Buffered;
using KworkNotify.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;

namespace KworkNotify.Api.Controllers;

[Authorize]
[Route("backup")]
[ApiController]
public class BackupController(IOptions<AppSettings> settings) : ControllerBase
{
    [HttpGet("create")]
    public async Task<ActionResult> CreateBackup()
    {
        try
        {
            var result = await Cli.Wrap("bash")
                .WithArguments(settings.Value.BackupScriptPath)
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .ExecuteBufferedAsync();
            
            var stdout = result.StandardOutput;
            var stderr = result.StandardError;
            
            if (!string.IsNullOrEmpty(stderr))
            {
                return BadRequest(new { Output = stdout, Error = stderr });
            }
            
            return Ok(new
            {
                Output = stdout
            });
        }
        catch (Exception e)
        {
            const string error = "create backup failed";
            Log.ForContext<BackupController>().Error(e, error);
            return StatusCode(500, error);
        }
    }
}