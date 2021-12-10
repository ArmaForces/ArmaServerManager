using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Hangfire;

// Disable call not awaited
#pragma warning disable 1998

namespace ArmaForces.ArmaServerManager.Tests.Helpers.Dummy
{
    /// <summary>
    /// Class for <see cref="IJobScheduler"/> testing purposes.
    /// </summary>
    public class DummyClass
    {
        /// <summary>
        /// Does nothing.
        /// Needed for <see cref="IJobScheduler"/> tests.
        /// </summary>
        public async Task DoNothing(CancellationToken cancellationToken)
        {
        }

        /// <summary>
        /// Does nothing and even more nothing.
        /// Needed for <see cref="IJobScheduler"/> tests.
        /// </summary>
        public async Task DoNothingAndNothing(CancellationToken cancellationToken)
        {
        }
    }
}
