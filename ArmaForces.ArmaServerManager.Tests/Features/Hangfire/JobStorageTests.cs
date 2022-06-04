using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using ArmaForces.ArmaServerManager.Features.Hangfire.Persistence;
using Hangfire.Storage;
using Moq;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Hangfire
{
    public class JobStorageTests
    {
        [Fact]
        public void GetJobDetails()
        {
            var hangfireDataAccess = Mock.Of<IHangfireDataAccess>();
            var monitoringApi = Mock.Of<IMonitoringApi>();
            var storageConnection = Mock.Of<IStorageConnection>();
            
            var jobStorage = new JobStorage(hangfireDataAccess, monitoringApi, storageConnection);
            
            jobStorage.GetJobDetails("123");
        }
    }
}
