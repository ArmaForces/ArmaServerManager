using Arma.Server.Features.Server;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arma.Server.Test.Features.Server
{
    public class ServerProcessTests
    {
        [Fact]
        public void IsStopped_ServerNotStarted_ReturnsTrue()
        {
            const string exePath = "";
            const string arguments = "";

            var serverProcess = new ServerProcess(
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

            var serverProcess = new ServerProcess(
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
            const string expectedError = "Arma 3 Server could not be started.";

            var serverProcess = new ServerProcess(
                exePath,
                arguments,
                new Logger<ServerProcess>(new LoggerFactory()));

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

            var serverProcess = new ServerProcess(
                exePath,
                arguments,
                new Logger<ServerProcess>(new LoggerFactory()));

            var startServerResult = serverProcess.Shutdown();

            using (new AssertionScope())
            {
                startServerResult.IsSuccess.Should().BeTrue();
            }
        }
    }
}
