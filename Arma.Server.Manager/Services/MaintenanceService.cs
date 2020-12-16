using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Features.Hangfire;
using CSharpFunctionalExtensions;
using Hangfire;

namespace Arma.Server.Manager.Services
{
    public class MaintenanceService
    {
        private readonly IHangfireManager _hangfireManager;

        public MaintenanceService(IHangfireManager hangfireManager)
        {
            _hangfireManager = hangfireManager;
        }

        public async Task<Result> PerformMaintenance(CancellationToken cancellationToken)
        {
            var serverShutdownJob = _hangfireManager.ScheduleJob<ServerStartupService>(x => x.ShutdownServer(2302, false, CancellationToken.None));

            var missionsPreparationJob = BackgroundJob.ContinueJobWith<IMissionPreparationService>(
                serverShutdownJob.Value,
                x => x.PrepareForUpcomingMissions(CancellationToken.None));

            var startServerForNextMissionJob = BackgroundJob.ContinueJobWith<IMissionPreparationService>(
                missionsPreparationJob,
                x => x.StartServerForNearestMission(CancellationToken.None));

            return Result.Success();
        }
    }
}
