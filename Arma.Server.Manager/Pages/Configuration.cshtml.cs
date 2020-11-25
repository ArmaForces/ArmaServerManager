using Arma.Server.Config;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Arma.Server.Manager.Pages
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

        public void OnPost()
        {
        }
    }
}
