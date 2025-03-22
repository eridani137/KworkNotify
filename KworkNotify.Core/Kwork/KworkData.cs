using KworkNotify.Core.Interfaces;

namespace KworkNotify.Core.Kwork;

public class KworkData : IKworkData
{
    public required string Cookies { get; init; }
}