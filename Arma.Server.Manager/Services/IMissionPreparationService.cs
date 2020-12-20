using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Services
{
    public interface IMissionPreparationService
    {
        /// <summary>
        /// Runs modifications preparation for upcoming missions.
        /// </summary>
        Task<Result> PrepareForUpcomingMissions(CancellationToken cancellationToken);

        /// <summary>
        /// Starts server for nearest upcoming mission.
        /// </summary>
        Task<Result> StartServerForNearestMission(CancellationToken cancellationToken);
    }
}
