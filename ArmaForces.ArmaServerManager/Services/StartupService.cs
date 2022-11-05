using System;
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

        /// <inheritdoc cref="StartupService"/>
        public StartupService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_webHostEnvironment.IsProduction())
            {
                RecurringJob.AddOrUpdate<MaintenanceService>(x => x.PerformMaintenance(CancellationToken.None),
                    Cron.Daily(4));
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
