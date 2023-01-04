using System.Linq;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Extensions
{

    internal static class JobsExtensions
    {
        public static object? GetParameterValue(this JobDetails? jobDetails, string parameterName)
            => jobDetails?.Parameters.SingleOrDefault(x => x.Key == parameterName);
    }
}