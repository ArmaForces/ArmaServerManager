using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Pages
{
    public class ConfigurationModel : PageModel
    {
        private readonly ILogger<ConfigurationModel> _logger;

        public ISettings Settings { get; set; }

        public ConfigurationModel(ISettings settings, ILogger<ConfigurationModel> logger)
        {
            Settings = settings;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task OnPost(Settings settings)
        {
            await Settings.ReloadSettings(settings);
        }
    }
}
