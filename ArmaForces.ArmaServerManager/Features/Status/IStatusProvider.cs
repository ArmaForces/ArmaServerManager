using System.Collections.Generic;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Api.Status;
using ArmaForces.ArmaServerManager.Api.Status.DTOs;
using ArmaForces.ArmaServerManager.Features.Status.Models;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Status
{
    public interface IStatusProvider
    {
        Task<Result<AppStatusDetails>> GetAppStatus(IEnumerable<AppStatusIncludes> includeJobs);
    }
}