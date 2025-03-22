using KworkNotify.Core.Telegram;
using Microsoft.AspNetCore.Mvc;

namespace KworkNotify.Api.Interfaces;

public interface ITelegramBotController
{
    Task<ActionResult<IEnumerable<TelegramUser>>> GetAllUsers();
    Task<ActionResult<TelegramUser>> GetUser([FromQuery] long id);
    Task<IActionResult> UpdateUser([FromQuery] long id, [FromBody] TelegramUser? updatedUser);
    Task<ActionResult<bool>> DeleteUser([FromQuery] long id);
}