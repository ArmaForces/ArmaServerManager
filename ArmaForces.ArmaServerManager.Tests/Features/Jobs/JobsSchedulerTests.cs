using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Tests.Helpers.Extensions;
using ArmaForces.ArmaServerManager.Features.Jobs;
using ArmaForces.ArmaServerManager.Features.Jobs.Helpers;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence;
using ArmaForces.ArmaServerManager.Tests.Helpers.Dummy;
using FluentAssertions.Execution;
using Hangfire.Common;
using Hangfire.Storage.Monitoring;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Jobs
{
    [Trait("Category", "Unit")]
    public class JobSchedulerTests
    {
        private const string DummyClassFirstMethodName = "DoNothing";
        private const string DummyClassSecondMethodName = "DoNothingAndNothing";

        private const string JobAlreadyInQueueError = "Similar job is already in queue.";

        private readonly Mock<IHangfireBackgroundJobClientWrapper> _backgroundJobClientMock =
            new Mock<IHangfireBackgroundJobClientWrapper>();

        private readonly Mock<IJobsRepository> _hangfireJobStorageMock = new Mock<IJobsRepository>();

        private readonly IJobsScheduler _jobsScheduler;

        public JobSchedulerTests()
        {
            _jobsScheduler = new JobsScheduler(_backgroundJobClientMock.Object, _hangfireJobStorageMock.Object, new NullLogger<JobsScheduler>());

            PrepareJobStorageMock();
        }

        [Fact (Skip = "This scenario is not relevant for unit test.")]
        public void ScheduleJob_OtherJobExistsDateTimeNull_JobQueued()
        {
            var otherScheduledJob =
                PrepareQueuedOrScheduledJob<ScheduledJobDto, DummyClass>(DummyClassSecondMethodName, DateTime.Now);
            AddScheduledJobs<DummyClass>(otherScheduledJob.AsList());

            var result = _jobsScheduler.ScheduleJob<DummyClass>(x => x.DoNothing(CancellationToken.None));

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();
                AssertScheduleCalled(Times.Never());
                AssertEnqueueCalled();
            }
        }

        [Fact]
        public void ScheduleJob_JobScheduledIn10SecondsDateTimeNow_JobAlreadyQueued()
        {
            var dateTime = DateTime.Now;
            var scheduledJobDateTime = dateTime.AddSeconds(10);
            var expectedError = $"Similar job is already scheduled at {scheduledJobDateTime}.";

            var otherScheduledJob =
                PrepareQueuedOrScheduledJob<ScheduledJobDto, DummyClass>(
                    nameof(DummyClass.DoNothing),
                    scheduledJobDateTime);
            AddScheduledJobs<DummyClass>(otherScheduledJob.AsList());

            var result = _jobsScheduler.ScheduleJob<DummyClass>(
                x => x.DoNothing(CancellationToken.None),
                dateTime);

            using (new AssertionScope())
            {
                result.ShouldBeFailure(expectedError);
                AssertScheduleCalled(Times.Never());
                AssertEnqueueCalled(Times.Never());
            }
        }

        [Fact]
        public void ScheduleJob_JobScheduledDateTimeNull_JobAlreadyScheduled()
        {
            var scheduledJob =
                PrepareQueuedOrScheduledJob<ScheduledJobDto, DummyClass>(DummyClassFirstMethodName, DateTime.Now);
            AddScheduledJobs<DummyClass>(scheduledJob.AsList());

            var result = _jobsScheduler.ScheduleJob<DummyClass>(x => x.DoNothing(CancellationToken.None));

            using (new AssertionScope())
            {
                result.ShouldBeFailure(JobAlreadyInQueueError);
                AssertEnqueueCalled(Times.Never());
                AssertEnqueueCalled(Times.Never());
            }
        }

        [Fact]
        public void ScheduleJob_JobScheduledDateTimeNow_JobAlreadyScheduled()
        {
            var dateTime = DateTime.Now;
            var expectedError = $"Similar job is already scheduled at {dateTime}.";

            var scheduledJob =
                PrepareQueuedOrScheduledJob<ScheduledJobDto, DummyClass>(DummyClassFirstMethodName, dateTime);
            AddScheduledJobs<DummyClass>(scheduledJob.AsList());

            var result = _jobsScheduler.ScheduleJob<DummyClass>(
                x => x.DoNothing(CancellationToken.None),
                dateTime);

            using (new AssertionScope())
            {
                result.ShouldBeFailure(expectedError);
                AssertScheduleCalled(Times.Never());
                AssertEnqueueCalled(Times.Never());
            }
        }

        [Fact]
        public void ScheduleJob_JobScheduledDateTimeInFuture_JobScheduled()
        {
            var dateTime = DateTime.Now.AddDays(1);

            var scheduledJob = PrepareQueuedOrScheduledJob<ScheduledJobDto, DummyClass>(DummyClassFirstMethodName, DateTime.Now);
            AddScheduledJobs<DummyClass>(scheduledJob.AsList());

            var result = _jobsScheduler.ScheduleJob<DummyClass>(
                x => x.DoNothing(CancellationToken.None),
                dateTime);

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();
                AssertScheduleCalled();
                AssertEnqueueCalled(Times.Never());
            }
        }

        [Fact]
        public void ScheduleJob_JobQueuedDateTimeNull_JobAlreadyQueued()
        {
            var queuedJob =
                PrepareQueuedOrScheduledJob<EnqueuedJobDto, DummyClass>(DummyClassFirstMethodName, DateTime.Now);
            AddQueuedJobs<DummyClass>(queuedJob.AsList());

            var result = _jobsScheduler.ScheduleJob<DummyClass>(x => x.DoNothing(CancellationToken.None));

            using (new AssertionScope())
            {
                result.ShouldBeFailure(JobAlreadyInQueueError);
                AssertScheduleCalled(Times.Never());
                AssertEnqueueCalled(Times.Never());
            }
        }

        [Fact]
        public void ScheduleJob_JobQueuedDateTimeNow_JobAlreadyQueued()
        {
            var dateTime = DateTime.Now;

            var queuedJob = PrepareQueuedOrScheduledJob<EnqueuedJobDto, DummyClass>(DummyClassFirstMethodName, DateTime.Now);
            AddQueuedJobs<DummyClass>(queuedJob.AsList());

            var result = _jobsScheduler.ScheduleJob<DummyClass>(
                x => x.DoNothing(CancellationToken.None),
                dateTime);

            using (new AssertionScope())
            {
                result.ShouldBeFailure(JobAlreadyInQueueError);
                AssertScheduleCalled(Times.Never());
                AssertEnqueueCalled(Times.Never());
            }
        }

        private void AssertEnqueueCalled(Times? times = null)
            => _backgroundJobClientMock.Verify(
                x => x.Enqueue<DummyClass>(dummyClass => dummyClass.DoNothing(CancellationToken.None)),
                times ?? Times.Once());

        private void AssertScheduleCalled(Times? times = null)
            => _backgroundJobClientMock.Verify(
                x => x.Schedule<DummyClass>(
                    dummyClass => dummyClass.DoNothing(CancellationToken.None),
                    It.IsAny<DateTimeOffset>()),
                times ?? Times.Once());

        private void PrepareJobStorageMock()
        {
            _hangfireJobStorageMock
                .Setup(
                    x => x.GetSimilarQueuedJobs(
                        It.IsAny<Expression<Func<DummyClass, Task>>>(),
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<int>()))
                .Returns(new List<EnqueuedJobDto>());

            _hangfireJobStorageMock
                .Setup(
                    x => x.GetSimilarScheduledJobs(
                        It.IsAny<Expression<Func<DummyClass, Task>>>(),
                        It.IsAny<int>(),
                        It.IsAny<int>()))
                .Returns(new List<ScheduledJobDto>());
        }

        private void AddScheduledJobs<T>(IReadOnlyCollection<ScheduledJobDto> scheduledJobs)
            => _hangfireJobStorageMock.Setup(x => x.GetSimilarScheduledJobs(
                    It.IsAny<Expression<Func<T,Task>>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(scheduledJobs.ToList);

        private void AddQueuedJobs<T>(IReadOnlyCollection<EnqueuedJobDto> queuedJobs)
            => _hangfireJobStorageMock.Setup(
                    x => x.GetSimilarQueuedJobs(
                        It.IsAny<Expression<Func<T,Task>>>(),
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<int>()))
                .Returns(queuedJobs.ToList);

        private static TJob PrepareQueuedOrScheduledJob<TJob, TType>(string methodName, DateTime dateTime)
            where TJob : class, new()
            => PrepareQueuedOrScheduledJob<TJob>(PrepareJob<TType>(methodName), dateTime);

        private static TJob PrepareQueuedOrScheduledJob<TJob>(Job job, DateTime dateTime) where TJob : class, new()
        {
            if (typeof(TJob) == typeof(ScheduledJobDto))
                return PrepareScheduledJob(job, dateTime) as TJob ?? throw new InvalidOperationException();

            if (typeof(TJob) == typeof(EnqueuedJobDto))
                return PrepareQueuedJob(job, dateTime) as TJob ?? throw new InvalidOperationException();

            return new TJob();
        }

        private static EnqueuedJobDto PrepareQueuedJob(Job job, in DateTime dateTime)
            => new EnqueuedJobDto
            {
                EnqueuedAt = dateTime,
                Job = job,
                InEnqueuedState = true
            };

        private static ScheduledJobDto PrepareScheduledJob(Job job, DateTime dateTime)
            => new ScheduledJobDto
            {
                EnqueueAt = dateTime,
                ScheduledAt = dateTime,
                Job = job,
                InScheduledState = true
            };

        private static Job PrepareJob<T>(string methodName)
            => new Job(
                typeof(T),
                typeof(T).GetMethod(methodName),
                CancellationToken.None);
    }
}
