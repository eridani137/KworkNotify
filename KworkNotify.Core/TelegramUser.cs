namespace KworkNotify.Core;

public class TelegramUser
{
    public long Id { get; init; }
    public TelegramRole Role { get; set; }
    public bool SendUpdates { get; set; }
}