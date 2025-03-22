using KworkNotify.Core.Dto;
using KworkNotify.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace KworkNotify.Api.Interfaces;

public interface IAuthController
{
    Task<IActionResult> GenerateJwtToken([FromBody] LoginDto login);
    string GenerateJwtToken(ApiUser user);
}