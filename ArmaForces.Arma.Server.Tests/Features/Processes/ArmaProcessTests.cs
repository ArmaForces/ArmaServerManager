using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Tests.Helpers.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Features.Processes
{
    [Trait("Category", "Unit")]
    public class ArmaProcessTests
    {
        [Fact]
        public void IsStopped_ServerNotStarted_ReturnsTrue()
        {
            var serverProcess = CreateArmaProcess();

            serverProcess.IsStopped.Should().BeTrue();
        }

        [Fact]
        public void IsStarting_ServerNotStarted_ReturnsFalse()
        {
            var serverProcess = CreateArmaProcess();

            serverProcess.IsStartingOrStarted.Should().BeFalse();
        }

        [Fact]
        public void Start_WrongExecutablePath_ReturnsResultFailure()
        {
            const string expectedError = "Arma 3 process could not be started.";

            var serverProcess = CreateArmaProcess();

            var startServerResult = serverProcess.Start();

            startServerResult.ShouldBeFailure(expectedError);
        }
        
        [Fact]
        public async Task Shutdown_ProcessNotStarted_ReturnsResultSuccess()
        {
            const string expectedErrorMessage = "Process could not be shut down because it's not running.";

            var serverProcess = CreateArmaProcess();

            var shutdownResult = await serverProcess.Shutdown();

            shutdownResult.ShouldBeFailure(expectedErrorMessage);
        }

        private static ArmaProcess CreateArmaProcess(
            string? exePath = null,
            string? arguments = null)
            => new ArmaProcess(
                exePath ?? string.Empty,
                arguments ?? string.Empty,
                new NullLogger<ArmaProcess>());
    }
}
