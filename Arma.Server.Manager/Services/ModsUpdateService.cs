using System;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Config;
using Arma.Server.Manager.Mods;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arma.Server.Manager.Services
{
    /// <summary>
    ///     Schedules recurring job for updating all cached mods.
    /// </summary>
    public class ModsUpdateService : IHostedService
    {
        private readonly IModsManager _modsManager;
        private readonly ISettings _settings;
        private bool _lock;

        public ModsUpdateService(ISettings settings, IModsManager modsManager)
        {
            _settings = settings;
            _modsManager = modsManager;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            RecurringJob.AddOrUpdate<ModsUpdateService>(x => x.Update(CancellationToken.None), Cron.Hourly);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public static ModsUpdateService CreateModsUpdateService(IServiceProvider serviceProvider)
            => new ModsUpdateService(
                serviceProvider.GetService<ISettings>(),
                serviceProvider.GetService<IModsManager>());

        /// <summary>
        ///     Handles updating all cached mods.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for safe process abort.</param>
        /// <returns>Awaitable <see cref="Task" /></returns>
        [MaximumConcurrentExecutions(1)]
        public async Task Update(CancellationToken cancellationToken)
            => await _modsManager.UpdateAllMods(cancellationToken);
    }
}
