using System;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Mods;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arma.Server.Manager.Services
{
    public class StartupService : IHostedService
    {
        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            RecurringJob.AddOrUpdate<ModsUpdateService>(x => x.UpdateAllMods(CancellationToken.None), Cron.Hourly);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
