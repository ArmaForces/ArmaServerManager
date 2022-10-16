﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Jobs;
using ArmaForces.ArmaServerManager.Features.Servers;
using ArmaForces.ArmaServerManager.Features.Servers.Providers;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ArmaForces.ArmaServerManager.Services
{
    public class StartupService : IHostedService
    {
        private readonly IJobsScheduler _jobsScheduler;
        private readonly IServerProvider _serverProvider;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StartupService(IJobsScheduler jobsScheduler, IServerProvider serverProvider, IWebHostEnvironment webHostEnvironment)
        {
            _jobsScheduler = jobsScheduler;
            _serverProvider = serverProvider;
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
