using System.Linq;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Extensions
{

    internal static class JobsExtensions
    {
        public static object? GetParameterValue(this JobDetails? jobDetails, string parameterName)
            => jobDetails?.Parameters.SingleOrDefault(x => x.Key == parameterName);
    }
}