using KworkNotify.Core.Kwork;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace KworkNotify.Core.Telegram.Forms;

public class ProjectForm : FormBase
{
    public TelegramUser? User { get; set; }
    public KworkProject? Project { get; set; }
    public ProjectForm()
    {
        Opened += OnOpened;
    }
    private Task OnOpened(object sender, EventArgs e)
    {
        if (User == null || Project == null) return Task.CompletedTask;
        
        var form = new ButtonForm();
        
        form.AddButtonRow(new ButtonBase("test", "test"));

        var grid = new ButtonGrid(form);
        grid.Title = Project.ToString();
        
        AddControl(grid);
        return Task.CompletedTask;
    }
}