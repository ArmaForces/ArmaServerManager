using Hangfire;
using Hangfire.Storage;

namespace Arma.Server.Manager.Features.Hangfire.Helpers {
    public class HangfireJobStorage : IHangfireJobStorage {
        private readonly JobStorage _jobStorage = JobStorage.Current;

        public IMonitoringApi MonitoringApi { get; }

        public HangfireJobStorage() {
            MonitoringApi = _jobStorage.GetMonitoringApi();
        }
    }
}
