using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ArmaForces.ArmaServerManager.Services
{
    /// <summary>
    /// Handles manager startup stuff.
    /// Initializes recurring jobs on production.
    /// </summary>
    public class StartupService : IHostedService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWebhookService _webhookService;

        /// <inheritdoc cref="StartupService"/>
        public StartupService(IWebHostEnvironment webHostEnvironment, IWebhookService webhookService)
        {
            _webHostEnvironment = webHostEnvironment;
            _webhookService = webhookService;
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_webHostEnvironment.IsProduction())
            {
                RecurringJob.AddOrUpdate<MaintenanceService>(x => x.PerformMaintenance(CancellationToken.None),
                    Cron.Daily(4));
            }

            await _webhookService.AnnounceStart();
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
