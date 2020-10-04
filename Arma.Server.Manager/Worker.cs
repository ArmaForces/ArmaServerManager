using System;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Services;
using Hangfire;
using Microsoft.Extensions.Hosting;

namespace Arma.Server.Manager {
    public class Worker : IHostedService {
        public Task StartAsync(CancellationToken cancellationToken) {
            ScheduleModsPreparation();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        private Task ScheduleModsPreparation() {
            RecurringJob.AddOrUpdate<ModsUpdateService>(x => x.Update(CancellationToken.None), Cron.Hourly);
            return Task.CompletedTask;
        }
    }
}