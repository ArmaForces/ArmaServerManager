using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Status.Models;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Status
{
    public interface IStatusProvider
    {
        Task<Result<AppStatus>> GetAppStatus(bool includeJobs, bool includeServers);
    }
}