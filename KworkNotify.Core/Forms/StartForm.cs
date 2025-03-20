using TelegramBotBase.Form;

namespace KworkNotify.Core.Forms;

public class StartForm : FormBase
{
    public StartForm()
    {
        Opened += OnOpened;
    }
    private async Task OnOpened(object sender, EventArgs e)
    {
        await Device.Send("Hello!");
    }
}