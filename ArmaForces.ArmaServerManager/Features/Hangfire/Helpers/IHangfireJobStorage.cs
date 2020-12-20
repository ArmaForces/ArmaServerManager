using Hangfire.Storage;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Helpers {
    public interface IHangfireJobStorage {
        IMonitoringApi MonitoringApi { get; }
    }
}
