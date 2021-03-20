using ArmaForces.Arma.Server.Features.Processes;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Features.Processes
{
    public class ArmaProcessTests
    {
        [Fact]
        public void IsStopped_ServerNotStarted_ReturnsTrue()
        {
            const string exePath = "";
            const string arguments = "";

            var serverProcess = new ArmaProcess(
                exePath,
                arguments,
                null);

            using (new AssertionScope())
            {
                serverProcess.IsStopped.Should().BeTrue();
            }
        }

        [Fact]
        public void IsStarting_ServerNotStarted_ReturnsFalse()
        {
            const string exePath = "";
            const string arguments = "";

            var serverProcess = new ArmaProcess(
                exePath,
                arguments,
                null);

            using (new AssertionScope())
            {
                serverProcess.IsStartingOrStarted.Should().BeFalse();
            }
        }

        [Fact]
        public void Start_WrongExecutablePath_ReturnsResultFailure()
        {
            const string exePath = "";
            const string arguments = "";
            const string expectedError = "Arma 3 process could not be started.";

            var serverProcess = new ArmaProcess(
                exePath,
                arguments,
                new Logger<ArmaProcess>(new NullLoggerFactory()));

            var startServerResult = serverProcess.Start();

            using (new AssertionScope())
            {
                startServerResult.IsSuccess.Should().BeFalse();
                startServerResult.Error.Should().Be(expectedError);
            }
        }


        [Fact]
        public void Shutdown_ProcessNotStarted_ReturnsResultSuccess()
        {
            const string exePath = "";
            const string arguments = "";

            var serverProcess = new ArmaProcess(
                exePath,
                arguments,
                new Logger<ArmaProcess>(new NullLoggerFactory()));

            var shutdownResult = serverProcess.Shutdown();

            using (new AssertionScope())
            {
                shutdownResult.IsSuccess.Should().BeFalse("Server is not running.");
            }
        }
    }
}
