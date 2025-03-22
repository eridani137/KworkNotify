namespace KworkNotify.Core.Interfaces;

public interface IJwtSettings
{
    string Secret { get; init; }
    string Issuer { get; init; }
    string Audience { get; init; }
}