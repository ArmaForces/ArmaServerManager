using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Services
{
    /// <summary>
    /// Handles preparation of server for missions.
    /// </summary>
    public interface IMissionPreparationService
    {
        /// <summary>
        /// Runs modifications preparation for upcoming missions.
        /// </summary>
        Task<UnitResult<IError>> PrepareForUpcomingMissions(CancellationToken cancellationToken);

        /// <summary>
        /// Starts server for nearest upcoming mission.
        /// </summary>
        Task<UnitResult<IError>> StartServerForNearestMission(CancellationToken cancellationToken);
    }
}
