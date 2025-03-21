namespace KworkNotify.Core.Telegram;

public class TelegramUser
{
    public long Id { get; init; }
    public TelegramRole Role { get; set; }
    public bool SendUpdates { get; set; }
}