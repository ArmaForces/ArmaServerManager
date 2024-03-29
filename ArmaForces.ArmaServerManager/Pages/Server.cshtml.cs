using System;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Api.Servers.DTOs;
using ArmaForces.ArmaServerManager.Features.Jobs;
using ArmaForces.ArmaServerManager.Features.Missions;
using ArmaForces.ArmaServerManager.Features.Missions.DTOs;
using ArmaForces.ArmaServerManager.Features.Modsets.Client;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
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
        private readonly IJobsScheduler _jobsScheduler;
        private readonly ILogger<ServerModel> _logger;
        
        public SelectList? ModsetOptions { get; set; }
      
        public SelectList? MissionOptions { get; set; }

        [BindProperty]
        public string? ModsetName { get; set; }

        [BindProperty]
        public string? MissionTitle { get; set; }

        public ServerModel(
            IApiModsetClient apiModsetClient,
            IApiMissionsClient apiMissionsClient,
            IJobsScheduler jobsScheduler,
            ILogger<ServerModel> logger)
        {
            _apiModsetClient = apiModsetClient;
            _apiMissionsClient = apiMissionsClient;
            _jobsScheduler = jobsScheduler;
            _logger = logger;
        }

        public async Task OnGet()
        {
            await LoadModsets();
            await LoadMissions();
        }

        public async Task OnPost()
        {
            if (ModsetName != null)
            {
                _jobsScheduler.ScheduleJob<ServerStartupService>(
                    x => x.StartServer(ModsetName, ServerStartRequestDto.DefaultHeadlessClients, CancellationToken.None));
            }
            else
            {
                if (MissionTitle is null)
                {
                    throw new Exception("Mission with no name selected.");
                }

                _jobsScheduler.ScheduleJob<ServerStartupService>(
                    x => x.StartServerForMission(MissionTitle, CancellationToken.None));
            }

            // Reload all data to prevent user having to refresh
            await OnGet();
        }

        private async Task LoadModsets()
        {
            var modsetsResult = await _apiModsetClient.GetModsets();
            if (modsetsResult.IsFailure) return;

            var modsets = modsetsResult.Value;
            ModsetOptions = new SelectList(modsets, nameof(WebModset.Name), nameof(WebModset.Name));
        }

        private async Task LoadMissions()
        {
            var missionsResult = await _apiMissionsClient.GetUpcomingMissions();
            if (missionsResult.IsFailure) return;

            var missions = missionsResult.Value;
            MissionOptions = new SelectList(missions, nameof(WebMission.Title), nameof(WebMission.Title));
        }
    }
}
