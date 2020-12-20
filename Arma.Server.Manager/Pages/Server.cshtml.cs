using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Clients.Missions;
using Arma.Server.Manager.Clients.Missions.Entities;
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
        private readonly IApiMissionsClient _apiMissionsClient;
        private readonly IHangfireManager _hangfireManager;
        private readonly ILogger<ServerModel> _logger;
        
        public SelectList ModsetOptions { get; set; }
      
        public SelectList MissionOptions { get; set; }

        [BindProperty]
        public string ModsetName { get; set; }

        [BindProperty]
        public string MissionTitle { get; set; }

        public ServerModel(
            IApiModsetClient apiModsetClient,
            IApiMissionsClient apiMissionsClient,
            IHangfireManager hangfireManager,
            ILogger<ServerModel> logger)
        {
            _apiModsetClient = apiModsetClient;
            _apiMissionsClient = apiMissionsClient;
            _hangfireManager = hangfireManager;
            _logger = logger;
        }

        public void OnGet()
        {
            var modsets = _apiModsetClient.GetModsets();
            ModsetOptions = new SelectList(modsets, nameof(WebModset.Name), nameof(WebModset.Name));

            var missions = _apiMissionsClient.GetUpcomingMissions();
            MissionOptions = new SelectList(missions, nameof(WebMission.Title), nameof(WebMission.Title));
        }

        public async Task OnPost()
        {
            if (ModsetName != null)
            {
                _hangfireManager.ScheduleJob<ServerStartupService>(
                    x => x.StartServer(ModsetName, CancellationToken.None));
            }
            else
            {
                _hangfireManager.ScheduleJob<ServerStartupService>(
                    x => x.StartServerForMission(MissionTitle, CancellationToken.None));
            }
        }
    }
}
