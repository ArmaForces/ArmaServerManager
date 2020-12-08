using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Features.Hangfire;
using Hangfire;
using Microsoft.Extensions.Hosting;

namespace Arma.Server.Manager.Services
{
    public class StartupService : IHostedService
    {
        private readonly IHangfireManager _hangfireManager;

        public StartupService(IHangfireManager hangfireManager)
        {
            _hangfireManager = hangfireManager;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            RecurringJob.AddOrUpdate<MaintenanceService>(x => x.PerformMaintenance(CancellationToken.None), Cron.Daily(4));
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
