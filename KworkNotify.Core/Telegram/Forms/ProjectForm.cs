using KworkNotify.Core.Interfaces;
using TelegramBotBase.Form;

namespace KworkNotify.Core.Telegram.Forms;

public class ProjectForm : FormBase
{
    private readonly IMongoContext _context;
    private readonly IAppCache _redis;
    private readonly IAppSettings _settings;
    private TelegramUser? _user;
    public ProjectForm(IMongoContext context, IAppCache redis, IAppSettings settings)
    {
        _context = context;
        _redis = redis;
        _settings = settings;
        Opened += OnOpened;
    }
    private async Task OnOpened(object sender, EventArgs e)
    {
        _user = await _context.OnOpenedForm(_redis, _settings, Device.DeviceId);
        if (_user is null) return;
        
        
    }
}