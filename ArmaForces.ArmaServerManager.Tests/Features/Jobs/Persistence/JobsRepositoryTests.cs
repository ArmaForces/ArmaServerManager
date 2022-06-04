using ArmaForces.ArmaServerManager.Features.Jobs.Helpers;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence;
using Hangfire.Storage;
using Moq;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Jobs.Persistence
{
    public class JobStorageTests
    {
        [Fact]
        public void GetJobDetails()
        {
            var backgroundJobClientWrapper = Mock.Of<IHangfireBackgroundJobClientWrapper>();
            var hangfireDataAccess = Mock.Of<IJobsDataAccess>();
            var monitoringApi = Mock.Of<IMonitoringApi>();
            var storageConnection = Mock.Of<IStorageConnection>();
            
            var jobStorage = new JobsRepository(backgroundJobClientWrapper, hangfireDataAccess, monitoringApi, storageConnection);
            
            jobStorage.GetJobDetails("123");
        }
    }
}
