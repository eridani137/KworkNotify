using KworkNotify.Core.Interfaces;

namespace KworkNotify.Core.Service.Types;

public class JwtSettings : IJwtSettings
{
    public required string Secret { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
}