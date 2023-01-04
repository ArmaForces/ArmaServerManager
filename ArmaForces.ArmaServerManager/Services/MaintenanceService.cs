using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Jobs;
using CSharpFunctionalExtensions;
using Hangfire;

namespace ArmaForces.ArmaServerManager.Services
{
    public class MaintenanceService
    {
        private readonly IJobsScheduler _jobsScheduler;

        public MaintenanceService(IJobsScheduler jobsScheduler)
        {
            _jobsScheduler = jobsScheduler;
        }

        public async Task<Result> PerformMaintenance(CancellationToken cancellationToken)
        {
            var serverShutdownJob = _jobsScheduler.ScheduleJob<ServerStartupService>(x => x.ShutdownServer(2302, false, CancellationToken.None));

            var missionsPreparationJob = BackgroundJob.ContinueJobWith<IMissionPreparationService>(
                serverShutdownJob.Value.ToString(),
                x => x.PrepareForUpcomingMissions(CancellationToken.None));

            var startServerForNextMissionJob = BackgroundJob.ContinueJobWith<IMissionPreparationService>(
                missionsPreparationJob,
                x => x.StartServerForNearestMission(CancellationToken.None));

            return Result.Success();
        }
    }
}
