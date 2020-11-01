using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Features.Hangfire;

namespace Arma.Server.Manager.Test.Helpers.Dummy
{
    /// <summary>
    /// Class for <see cref="IHangfireManager"/> testing purposes.
    /// </summary>
    public class DummyClass
    {
        /// <summary>
        /// Does nothing.
        /// Needed for <see cref="IHangfireManager"/> tests.
        /// </summary>
        public async Task DoNothing(CancellationToken cancellationToken)
        {
        }

        /// <summary>
        /// Does nothing and even more nothing.
        /// Needed for <see cref="IHangfireManager"/> tests.
        /// </summary>
        public async Task DoNothingAndNothing(CancellationToken cancellationToken)
        {
        }
    }
}
