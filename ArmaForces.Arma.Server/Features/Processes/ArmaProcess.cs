using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Parameters;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Processes
{
    public class ArmaProcess : IArmaProcess
    {
        private readonly ILogger<ArmaProcess> _logger;

        private Process? _serverProcess;

        // TODO: Remove after replacing tests
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public ArmaProcess(
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            string executablePath,
            string arguments,
            ILogger<ArmaProcess> logger)
        {
            _logger = logger;
        }

        public ArmaProcess(
            Process process,
            ProcessParameters processParameters,
            ILogger<ArmaProcess> logger) : this(processParameters, logger)
        {
            _serverProcess = process;
            _serverProcess.EnableRaisingEvents = true;
            _serverProcess.Exited += (sender, args) => InvokeOnProcessShutdown();
        }

        public ArmaProcess(
            ProcessParameters processParameters,
            ILogger<ArmaProcess> logger)
        {
            Parameters = processParameters;
            _logger = logger;
        }

        public ProcessParameters Parameters { get; }

        public ArmaProcessType ProcessType => Parameters.Client
            ? ArmaProcessType.HeadlessClient
            // TODO: Handle regular client process too to avoid shutting it down
            : ArmaProcessType.Server;

        public bool IsStopped => _serverProcess == null || _serverProcess.HasExited;

        public bool IsStartingOrStarted => !IsStopped;

        public event Func<IArmaProcess, Task>? OnProcessShutdown;

        public Result Start()
        {
            if (!IsStopped) return Result.Success();

            try
            {
                var processStartInfo = Parameters.GetProcessStartInfo();
                
                _logger.LogTrace(
                    "Starting {ExecutablePath} with {Arguments}",
                    processStartInfo.FileName,
                    processStartInfo.Arguments);

                _serverProcess = Process.Start(processStartInfo);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Arma 3 process could not be started");
                return Result.Failure("Arma 3 process could not be started.");
            }

            _serverProcess!.EnableRaisingEvents = true;
            _serverProcess.Exited += (sender, args) => InvokeOnProcessShutdown();

            _logger.LogInformation("Starting Arma 3 {ProcessType}", ProcessType);

            return Result.Success();
        }

        public Result Shutdown()
        {
            if (IsStopped || _serverProcess is null)
            {
                _logger.LogInformation("Server not running");
                return Result.Failure("Server could not be shut down because it's not running.");
            }

            _logger.LogDebug("Shutting down the {ArmaProcess}", _serverProcess);

            _serverProcess.Kill();
            _serverProcess = null;

            _logger.LogInformation("Server successfully shut down");

            return Result.Success();
        }

        private void InvokeOnProcessShutdown()
        {
            _logger.LogDebug("{ProcessType} process shutdown detected. Invoking OnProcessShutdown event", ProcessType);
            OnProcessShutdown?.Invoke(this);
        }
    }
}
