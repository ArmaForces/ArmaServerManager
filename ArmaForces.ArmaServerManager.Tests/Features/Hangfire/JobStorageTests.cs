using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Hangfire
{
    public class JobStorageTests
    {
        [Fact]
        public void GetJobDetails()
        {
            var jobStorage = new JobStorage();
            jobStorage.GetJobDetails("123");
        }
    }
}
