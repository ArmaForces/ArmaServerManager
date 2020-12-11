using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Clients.Modsets;
using Arma.Server.Manager.Clients.Modsets.Entities;
using Arma.Server.Manager.Features.Hangfire;
using Arma.Server.Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Arma.Server.Manager.Pages
{
    public class ServerModel : PageModel
    {
        private readonly IApiModsetClient _apiModsetClient;
        private readonly IHangfireManager _hangfireManager;
        private readonly ILogger<ServerModel> _logger;
        
        public SelectList Options { get; set; }

        public List<WebModset> Modsets { get; set; }

        [BindProperty]
        public string ModsetName { get; set; }

        public ServerModel(IApiModsetClient apiModsetClient, IHangfireManager hangfireManager, ILogger<ServerModel> logger)
        {
            _apiModsetClient = apiModsetClient;
            _hangfireManager = hangfireManager;
            _logger = logger;
        }

        public void OnGet()
        {
            Modsets = _apiModsetClient.GetModsets();
            Options = new SelectList(Modsets, nameof(WebModset.Name), nameof(WebModset.Name));
        }

        public async Task OnPost()
        {
            _hangfireManager.ScheduleJob<ServerStartupService>(x => x.StartServer(ModsetName, CancellationToken.None));
        }
    }
}
