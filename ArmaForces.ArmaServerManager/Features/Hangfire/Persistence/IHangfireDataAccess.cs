using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Hangfire.Models;
using ArmaForces.ArmaServerManager.Features.Hangfire.Persistence.Models;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Persistence
{
    internal interface IHangfireDataAccess
    {
        public List<JobDataModel> GetJobs(IEnumerable<JobStatus> includeStatuses);
    }
}