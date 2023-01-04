using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Jobs;

// Disable call not awaited
#pragma warning disable 1998

namespace ArmaForces.ArmaServerManager.Tests.Helpers.Dummy
{
    /// <summary>
    /// Class for <see cref="IJobsScheduler"/> testing purposes.
    /// </summary>
    public class DummyClass
    {
        /// <summary>
        /// Does nothing.
        /// Needed for <see cref="IJobsScheduler"/> tests.
        /// </summary>
        public async Task DoNothing(CancellationToken cancellationToken)
        {
        }

        /// <summary>
        /// Does nothing and even more nothing.
        /// Needed for <see cref="IJobsScheduler"/> tests.
        /// </summary>
        public async Task DoNothingAndNothing(CancellationToken cancellationToken)
        {
        }
    }
}
