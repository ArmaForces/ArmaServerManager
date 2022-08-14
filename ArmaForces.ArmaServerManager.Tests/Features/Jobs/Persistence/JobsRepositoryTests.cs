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

        [Theory]
        [ClassData(typeof(MethodWithParametersTestData))]
        public void GetJobDetails_MethodWithParameters_JobDetailsAreCorrect(MethodInfo methodInfo)
        {
            var jobId = _fixture.Create<int>().ToString();
            var jobData = CreateJobData(methodInfo);
            
            var expectedJobDetails = new JobDetails
            {
                Name = methodInfo.Name,
                CreatedAt = jobData.CreatedAt,
                JobStatus = JobStatusParser.ParseJobStatus(jobData.State),
                Parameters = jobData.Job.Method.GetParameters()
                    .Zip(jobData.Job.Args, (parameterInfo, parameterValue) => new KeyValuePair<string, object>(parameterInfo.Name ?? "unknown", parameterValue))
                    .Where(x => x.Key != "cancellationToken")
                    .ToList()
            };

            var backgroundJobClientWrapper = Mock.Of<IHangfireBackgroundJobClientWrapper>();
            var hangfireDataAccess = Mock.Of<IJobsDataAccess>();
            var monitoringApi = Mock.Of<IMonitoringApi>();
            var storageConnection = CreateMockedStorageConnection(new List<KeyValuePair<string, JobData>>
            {
                new KeyValuePair<string, JobData>(jobId, jobData)
            });
            
            var jobRepository = new JobsRepository(backgroundJobClientWrapper, hangfireDataAccess, monitoringApi, storageConnection);

            var result = jobRepository.GetJobDetails(jobId);
            
            result.ShouldBeSuccess(expectedJobDetails);
        }

        private JobData CreateJobData(
            MethodInfo? methodInfo,
            DateTime? createdAt = null,
            JobStatus status = JobStatus.Scheduled)
        {
            // methodInfo.GetParameters().First().ParameterType;

            var methodParameters = methodInfo?.GetParameters()
                .Select(x => _fixture.Create(x.ParameterType, new SpecimenContext(_fixture)))
                .ToArray()
                               ?? throw new ArgumentNullException(nameof(methodInfo));

            return new JobData
            {
                Job = new Job(
                    method: methodInfo,
                    methodParameters),
                CreatedAt = createdAt ?? _fixture.Create<DateTime>(),
                State = status.ToString()
            };
        }

        private static IStorageConnection CreateMockedStorageConnection(List<KeyValuePair<string, JobData>>? storedJobs = null)
        {
            storedJobs ??= new List<KeyValuePair<string, JobData>>();
            var mock = new Mock<IStorageConnection>();

            foreach (var storedJob in storedJobs)
            {
                mock
                    .Setup(storageConnection => storageConnection.GetJobData(storedJob.Key))
                    .Returns(storedJob.Value);
            }

            return mock.Object;
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
