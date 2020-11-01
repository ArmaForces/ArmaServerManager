using Hangfire.Storage;

namespace Arma.Server.Manager.Features.Hangfire.Helpers {
    public interface IHangfireJobStorage {
        IMonitoringApi MonitoringApi { get; }
    }
}
