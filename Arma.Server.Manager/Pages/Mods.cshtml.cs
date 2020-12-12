using System.Collections.Generic;
using System.Threading.Tasks;
using Arma.Server.Manager.Features.Mods;
using Arma.Server.Mod;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Arma.Server.Manager.Pages
{
    public class ModsModel : PageModel
    {
        private readonly IModsCache _modsCache;
        private readonly ILogger<ModsModel> _logger;
        
        public ISet<IMod> Mods { get; set; }

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
