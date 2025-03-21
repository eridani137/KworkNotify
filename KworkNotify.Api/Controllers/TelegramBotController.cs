using KworkNotify.Core.Service;
using KworkNotify.Core.Telegram;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Serilog;

namespace KworkNotify.Api.Controllers;

[Authorize]
[Route("tg")]
[ApiController]
public class TelegramBotController(MongoContext context) : ControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<TelegramUser>>> GetAllUsers()
    {
        try
        {
            var users = await context.Users.Find(_ => true).ToListAsync();
            return Ok(users);
        }
        catch (Exception e)
        {
            const string error = "An error occured while getting all telegram users";
            Log.ForContext<TelegramBotController>().Error(e, error);
            return StatusCode(500, error);
        }
    }
}