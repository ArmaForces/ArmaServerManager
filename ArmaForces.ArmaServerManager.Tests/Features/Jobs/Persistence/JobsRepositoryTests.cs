using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Tests.Helpers.Extensions;
using ArmaForces.ArmaServerManager.Features.Jobs.Helpers;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using ArmaForces.ArmaServerManager.Services;
using AutoFixture;
using AutoFixture.Kernel;
using Hangfire.Common;
using Hangfire.Storage;
using Moq;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Jobs.Persistence
{
    [Trait("Category", "Unit")]
    public class JobRepositoryTests
    {
        private readonly Fixture _fixture = new Fixture();

        public JobRepositoryTests()
        {
            _fixture.Customizations.Add(
                new TypeRelay(
                    typeof(IModset),
                    typeof(Modset)));
            
            _fixture.Customizations.Add(
                new TypeRelay(
                    typeof(IMod),
                    typeof(Mod)));
        }

        private Job CreateHangfireJob(MethodInfo methodInfo)
        {
            var methodParameters = methodInfo?.GetParameters()
                                       .Select(x => _fixture.Create(x.ParameterType, new SpecimenContext(_fixture)))
                                       .ToArray()
                                   ?? throw new ArgumentNullException(nameof(methodInfo));
            
            return new Job(
                method: methodInfo,
                methodParameters);
        }

        [Theory]
        [ClassData(typeof(MethodWithParametersTestData))]
        public void GetJobDetails_MethodWithParameters_JobDetailsAreCorrect(MethodInfo methodInfo)
        {
            var jobId = _fixture.Create<int>();
            var hangfireJob = CreateHangfireJob(methodInfo);
            var jobData = CreateJobDataModel(jobId, hangfireJob);
            
            var expectedJobDetails = new JobDetails
            {
                Id = jobId,
                Name = methodInfo.Name,
                CreatedAt = jobData.CreatedAt,
                JobStatus = jobData.JobStatus,
                Parameters = hangfireJob.Method.GetParameters()
                    .Zip(hangfireJob.Args, (parameterInfo, parameterValue) => new KeyValuePair<string, object>(parameterInfo.Name ?? "unknown", parameterValue))
                    .Where(x => x.Key != "cancellationToken")
                    .ToList()
            };

            var backgroundJobClientWrapper = Mock.Of<IHangfireBackgroundJobClientWrapper>();
            var monitoringApi = Mock.Of<IMonitoringApi>();
            var jobsDataAccess = CreateMockedJobsDataAccess(new List<KeyValuePair<int, JobDataModel>>
            {
                new KeyValuePair<int, JobDataModel>(jobId, jobData)
            });

            var jobRepository = new JobsRepository(backgroundJobClientWrapper, jobsDataAccess, monitoringApi);

            var result = jobRepository.GetJobDetails(jobId);
            
            result.ShouldBeSuccess(expectedJobDetails);
        }

        private IJobsDataAccess CreateMockedJobsDataAccess(List<KeyValuePair<int, JobDataModel>>? storedJobs)
        {
            storedJobs ??= new List<KeyValuePair<int, JobDataModel>>();
            var mock = new Mock<IJobsDataAccess>();

            foreach (var (jobId, jobDataModel) in storedJobs)
            {
                mock
                    .Setup(x => x.GetJob<JobDataModel>(jobId))
                    .Returns(jobDataModel);
            }
            
            return mock.Object;
        }

        private JobDataModel CreateJobDataModel(
            int jobId,
            Job job,
            DateTime? createdAt = null,
            JobStatus status = JobStatus.Scheduled)
        {
            return new JobDataModel
            {
                Id = jobId,
                JobStatus = status,
                CreatedAt = createdAt ?? _fixture.Create<DateTime>(),
                InvocationData = SerializationHelper.Serialize(InvocationData.SerializeJob(job)),
                Arguments = job.Args.ToString() ?? string.Empty
            };
        }

        private class MethodWithParametersTestData : IEnumerable<object[]>
        {
            private readonly IEnumerable<object[]> _enumerableImplementation;

            public MethodWithParametersTestData()
            {
                _enumerableImplementation = new[]
                    {
                        new object[]
                        {
                            typeof(MaintenanceService).GetMethod(nameof(MaintenanceService.PerformMaintenance))!
                        }
                    }
                    .Concat(typeof(ServerStartupService).GetMethods()
                        .Where(x => x.Name.Contains(nameof(ServerStartupService.StartServer)))
                        .Select(x => new object[] {x}))
                    .Concat(typeof(ServerStartupService).GetMethods()
                        .Where(x => x.Name.Contains(nameof(ServerStartupService.ShutdownServer)))
                        .Select(x => new object[] {x}))
                    .Concat(typeof(ModsUpdateService).GetMethods()
                        .Where(x => x.Name.Contains(nameof(ModsUpdateService.UpdateModset)))
                        .Select(x => new object[] {x}));
            }

            public IEnumerator<object[]> GetEnumerator() => _enumerableImplementation.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _enumerableImplementation).GetEnumerator();
        }
    }
}
