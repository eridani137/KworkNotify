using System.ComponentModel.DataAnnotations;

namespace KworkNotify.Core.Dto;

public class LoginDto
{
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string Password { get; set; }
}