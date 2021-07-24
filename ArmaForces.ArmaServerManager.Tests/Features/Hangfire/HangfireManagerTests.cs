using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ArmaForces.Arma.Server.Tests.Helpers.Extensions;
using ArmaForces.ArmaServerManager.Features.Hangfire;
using ArmaForces.ArmaServerManager.Features.Hangfire.Helpers;
using ArmaForces.ArmaServerManager.Tests.Helpers.Dummy;
using FluentAssertions.Execution;
using Hangfire.Common;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Moq;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Hangfire
{
    [Trait("Category", "Unit")]
    public class HangfireManagerTests
    {
        private const string DummyClassFirstMethodName = "DoNothing";
        private const string DummyClassSecondMethodName = "DoNothingAndNothing";

        private readonly Mock<IHangfireBackgroundJobClient> _backgroundJobClientMock =
            new Mock<IHangfireBackgroundJobClient>();

        private readonly Mock<IHangfireJobStorage> _hangfireJobStorageMock = new Mock<IHangfireJobStorage>();

        private readonly IHangfireManager _hangfireManager;
        private readonly Mock<IMonitoringApi> _monitoringApiMock;

        public HangfireManagerTests()
        {
            _hangfireManager = new HangfireManager(_backgroundJobClientMock.Object, _hangfireJobStorageMock.Object);

            _monitoringApiMock = PrepareMonitoringApiMock();
            _hangfireJobStorageMock.Setup(x => x.MonitoringApi).Returns(_monitoringApiMock.Object);
        }

        [Fact]
        public void ScheduleJob_OtherJobExistsDateTimeNull_JobQueued()
        {
            var otherScheduledJob =
                PrepareQueuedOrScheduledJob<ScheduledJobDto, DummyClass>(DummyClassSecondMethodName, DateTime.Now);
            var scheduledJobList = PrepareJobList(new[] {otherScheduledJob});
            AddScheduledJobs(scheduledJobList);

            var result = _hangfireManager.ScheduleJob<DummyClass>(x => x.DoNothing(CancellationToken.None));

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();
                AssertScheduleCalled(Times.Never());
                AssertEnqueueCalled();
            }
        }

        [Fact]
        public void ScheduleJob_JobScheduledInMinuteDateTimeNow_JobAlreadyQueued()
        {
            var dateTime = DateTime.Now;

            var otherScheduledJob =
                PrepareQueuedOrScheduledJob<ScheduledJobDto, DummyClass>(
                    "DoNothing",
                    DateTime.Now.AddMinutes(1));
            var scheduledJobList = PrepareJobList(new[] {otherScheduledJob});
            AddScheduledJobs(scheduledJobList);

            var result = _hangfireManager.ScheduleJob<DummyClass>(
                x => x.DoNothing(CancellationToken.None),
                dateTime);

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();
                AssertScheduleCalled(Times.Never());
                AssertEnqueueCalled(Times.Never());
            }
        }

        [Fact]
        public void ScheduleJob_JobScheduledDateTimeNull_JobAlreadyScheduled()
        {
            var scheduledJob =
                PrepareQueuedOrScheduledJob<ScheduledJobDto, DummyClass>(DummyClassFirstMethodName, DateTime.Now);
            var scheduledJobList = PrepareJobList(new[] {scheduledJob});
            AddScheduledJobs(scheduledJobList);

            var result = _hangfireManager.ScheduleJob<DummyClass>(x => x.DoNothing(CancellationToken.None));

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();
                AssertEnqueueCalled(Times.Never());
                AssertEnqueueCalled(Times.Never());
            }
        }

        [Fact]
        public void ScheduleJob_JobScheduledDateTimeNow_JobAlreadyScheduled()
        {
            var dateTime = DateTime.Now;

            var scheduledJob =
                PrepareQueuedOrScheduledJob<ScheduledJobDto, DummyClass>(DummyClassFirstMethodName, DateTime.Now);
            var scheduledJobList = PrepareJobList(new[] {scheduledJob});
            AddScheduledJobs(scheduledJobList);

            var result = _hangfireManager.ScheduleJob<DummyClass>(
                x => x.DoNothing(CancellationToken.None),
                dateTime);

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();
                AssertScheduleCalled(Times.Never());
                AssertEnqueueCalled(Times.Never());
            }
        }

        [Fact]
        public void ScheduleJob_JobScheduledDateTimeInFuture_JobScheduled()
        {
            var dateTime = DateTime.Now.AddDays(1);

            var scheduledJob =
                PrepareQueuedOrScheduledJob<ScheduledJobDto, DummyClass>(DummyClassFirstMethodName, DateTime.Now);
            var scheduledJobList = PrepareJobList(new[] {scheduledJob});
            AddScheduledJobs(scheduledJobList);

            var result = _hangfireManager.ScheduleJob<DummyClass>(
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
            var queuedJobList = PrepareJobList(new[] {queuedJob});
            AddQueuedJobs(queuedJobList);

            var result = _hangfireManager.ScheduleJob<DummyClass>(x => x.DoNothing(CancellationToken.None));

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();
                AssertScheduleCalled(Times.Never());
                AssertEnqueueCalled(Times.Never());
            }
        }

        [Fact]
        public void ScheduleJob_JobQueuedDateTimeNow_JobAlreadyQueued()
        {
            var dateTime = DateTime.Now;

            var queuedJob =
                PrepareQueuedOrScheduledJob<EnqueuedJobDto, DummyClass>(DummyClassFirstMethodName, DateTime.Now);
            var queuedJobList = PrepareJobList(new[] {queuedJob});
            AddQueuedJobs(queuedJobList);

            var result = _hangfireManager.ScheduleJob<DummyClass>(
                x => x.DoNothing(CancellationToken.None),
                dateTime);

            using (new AssertionScope())
            {
                result.ShouldBeSuccess();
                AssertEnqueueCalled(Times.Never());
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

        private static Mock<IMonitoringApi> PrepareMonitoringApiMock()
        {
            var monitoringApiMock = new Mock<IMonitoringApi>();

            monitoringApiMock.Setup(x => x.ScheduledJobs(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new JobList<ScheduledJobDto>(new KeyValuePair<string, ScheduledJobDto>[0]));

            monitoringApiMock.Setup(
                    x => x.EnqueuedJobs(
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<int>()))
                .Returns(new JobList<EnqueuedJobDto>(new KeyValuePair<string, EnqueuedJobDto>[0]));

            return monitoringApiMock;
        }

        private void AddScheduledJobs(JobList<ScheduledJobDto> scheduledJobs)
            => _monitoringApiMock.Setup(x => x.ScheduledJobs(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(scheduledJobs);

        private void AddQueuedJobs(JobList<EnqueuedJobDto> queuedJobs)
            => _monitoringApiMock.Setup(
                    x => x.EnqueuedJobs(
                        It.IsAny<string>(),
                        It.IsAny<int>(),
                        It.IsAny<int>()))
                .Returns(queuedJobs);

        private static JobList<T> PrepareJobList<T>(IEnumerable<T> jobsList) where T : new()
            => new JobList<T>(jobsList.Select(x => new KeyValuePair<string, T>("", x)));

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
