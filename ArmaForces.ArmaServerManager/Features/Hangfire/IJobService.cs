using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Hangfire
{
    public interface IJobService
    {
        Result<JobDetails> GetJobDetails(string jobId);
    }
}
