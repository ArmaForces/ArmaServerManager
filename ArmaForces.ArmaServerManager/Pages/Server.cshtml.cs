using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Clients.Missions;
using ArmaForces.ArmaServerManager.Clients.Missions.Entities;
using ArmaForces.ArmaServerManager.Clients.Modsets;
using ArmaForces.ArmaServerManager.Clients.Modsets.Entities;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Pages
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
