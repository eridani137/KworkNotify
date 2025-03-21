using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KworkNotify.Core;
using KworkNotify.Core.Auth;
using KworkNotify.Core.Dto;
using KworkNotify.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KworkNotify.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IUserService userService, IOptions<JwtSettings> jwtSettings) : ControllerBase
{
    [HttpPost("GenerateToken")]
    public async Task<IActionResult> GenerateJwtToken([FromBody] LoginDto login)
    {
        var user = await userService.Authenticate(login.Username, login.Password);
        if (user == null) return Unauthorized("Invalid username or password");
        var token = GenerateJwtToken(user);
        return Ok(new { Token = token });
    }
    
    private string GenerateJwtToken(ApiUser user)
    {
        var key = Encoding.UTF8.GetBytes(jwtSettings.Value.Secret);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Value.Issuer,
            audience: jwtSettings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}