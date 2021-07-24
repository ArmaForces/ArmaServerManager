using System;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Providers.Server;
using Hangfire;
using Microsoft.Extensions.Hosting;

namespace ArmaForces.ArmaServerManager.Services
{
    public class StartupService : IHostedService
    {
        private readonly IHangfireManager _hangfireManager;
        private readonly IServerProvider _serverProvider;

        public StartupService(IHangfireManager hangfireManager, IServerProvider serverProvider)
        {
            _hangfireManager = hangfireManager;
            _serverProvider = serverProvider;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //RecurringJob.AddOrUpdate<MaintenanceService>(x => x.PerformMaintenance(CancellationToken.None), Cron.Daily(4));

            // Initialize provider
            // TODO: Find a better way
            Task.Run(() =>
            {
                Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                return _ = _serverProvider.GetServer(2302);
            }, cancellationToken);
            

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
