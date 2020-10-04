using System;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Config;
using Arma.Server.Manager.Mods;
using Hangfire;
using Microsoft.Extensions.Hosting;

namespace Arma.Server.Manager.Services {
    /// <summary>
    /// Schedules recurring job for updating all cached mods.
    /// </summary>
    public class ModsUpdateService : IHostedService {
        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken) {
            RecurringJob.AddOrUpdate<ModsUpdateService>(x => x.Update(CancellationToken.None), Cron.Hourly);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles updating all cached mods.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for safe process abort.</param>
        /// <returns>Awaitable <see cref="Task"/></returns>
        public async Task Update(CancellationToken cancellationToken)
        {
            ISettings settings = new Settings();
            settings.LoadSettings();
            var modsManager = new ModsManager(settings);
            await modsManager.UpdateAllMods(cancellationToken);
        }
    }
}