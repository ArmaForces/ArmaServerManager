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
            var hangfireDataAccess = Mock.Of<IJobsDataAccess>();
            var monitoringApi = Mock.Of<IMonitoringApi>();
            var storageConnection = Mock.Of<IStorageConnection>();
            
            var jobStorage = new JobsRepository(hangfireDataAccess, monitoringApi, storageConnection);
            
            jobStorage.GetJobDetails("123");
        }
    }
}
