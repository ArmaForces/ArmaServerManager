using System.Collections.Generic;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Mods;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Pages
{
    public class ModsModel : PageModel
    {
        private readonly IModsCache _modsCache;
        private readonly ILogger<ModsModel> _logger;
        
        public IReadOnlyCollection<Mod>? Mods { get; set; }

        public ModsModel(
            IModsCache modsCache,
            ILogger<ModsModel> logger)
        {
            _modsCache = modsCache;
            _logger = logger;
        }

        public void OnGet()
        {
            Mods = _modsCache.Mods;
        }

        public async Task OnPost()
        {
        }
    }
}
