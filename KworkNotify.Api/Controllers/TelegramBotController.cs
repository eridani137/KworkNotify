using KworkNotify.Api.Interfaces;
using KworkNotify.Core.Interfaces;
using KworkNotify.Core.Telegram;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Serilog;

namespace KworkNotify.Api.Controllers;

[Authorize]
[Route("tg")]
[ApiController]
public class TelegramBotController(IMongoContext context) : ControllerBase, ITelegramBotController
{
    [HttpGet("getUsers")]
    public async Task<ActionResult<IEnumerable<TelegramUser>>> GetAllUsers()
    {
        try
        {
            var users = await context.Users.Find(_ => true).ToListAsync();
            if (users == null || users.Count == 0)
            {
                return NotFound("No users found");
            }
            return Ok(users);
        }
        catch (Exception e)
        {
            const string error = "An error occured while getting all telegram users";
            Log.ForContext<TelegramBotController>().Error(e, error);
            return StatusCode(500, error);
        }
    }
    [HttpGet("getUser")]
    public async Task<ActionResult<TelegramUser>> GetUser([FromQuery] long id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("Invalid user ID");
            }
            
            var user = await context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }
            return Ok(user);
        }
        catch (Exception e)
        {
            const string error = "An error occured while getting telegram user";
            Log.ForContext<TelegramBotController>().Error(e, error);
            return StatusCode(500, error);
        }
    }
    [HttpPut("updateUser")]
    public async Task<IActionResult> UpdateUser([FromQuery] long id, [FromBody] TelegramUser? updatedUser)
    {
        try
        {
            if (id <= 0 || updatedUser == null)
            {
                return BadRequest("Invalid user ID or user data");
            }

            if (id != updatedUser.Id)
            {
                return BadRequest("User ID in query and body do not match");
            }

            var existingUser = await context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (existingUser == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            var result = await context.Users.ReplaceOneAsync(u => u.Id == id, updatedUser);
            if (result.ModifiedCount == 0)
            {
                return StatusCode(500, "Failed to update user");
            }

            return Ok(updatedUser);
        }
        catch (Exception e)
        {
            const string error = "An error occurred while updating telegram user";
            Log.ForContext<TelegramBotController>().Error(e, error);
            return StatusCode(500, error);
        }
    }
    [HttpDelete("deleteUser")]
    public async Task<ActionResult<bool>> DeleteUser([FromQuery] long id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("Invalid user ID");
            }
            
            var result = await context.Users.DeleteOneAsync(u => u.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound($"User with ID {id} not found");
            }
            return Ok(true);
        }
        catch (Exception e)
        {
            const string error = "An error occured while deleting telegram user";
            Log.ForContext<TelegramBotController>().Error(e, error);
            return StatusCode(500, error);
        }
    }
}