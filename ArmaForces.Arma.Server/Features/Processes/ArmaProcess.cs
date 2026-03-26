using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Features.Parameters;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Processes
{
    public class ArmaProcess : IArmaProcess
    {
        private readonly ILogger<ArmaProcess> _logger;

        private Process? _process;

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
            _process = process;
            _process.EnableRaisingEvents = true;
            _process.Exited += (_, _) => InvokeOnProcessShutdown();
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

        public bool IsStopped => _process == null || _process.HasExited;

        public bool IsStartingOrStarted => !IsStopped;

        public DateTimeOffset? StartTime => Parameters.StartTime;

        public event Func<IArmaProcess, Task>? OnProcessShutdown;

        public UnitResult<IError> Start()
        {
            if (!IsStopped) return UnitResult.Success<IError>();

            try
            {
                var processStartInfo = Parameters.GetProcessStartInfo();
                
                _logger.LogTrace(
                    "Starting {ExecutablePath} with {Arguments}",
                    processStartInfo.FileName,
                    processStartInfo.Arguments);

                _process = Process.Start(processStartInfo);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Arma 3 process could not be started");
                return new Error("Arma 3 process could not be started.", ManagerErrorCode.ProcessStartFailed);
            }

            _process!.EnableRaisingEvents = true;
            _process.Exited += (_, _) => InvokeOnProcessShutdown();

            _logger.LogInformation("Starting Arma 3 {ProcessType}", ProcessType);

            return UnitResult.Success<IError>();
        }

        public async Task<UnitResult<IError>> Shutdown()
        {
            if (IsStopped || _process is null)
            {
                _logger.LogInformation("Process not running");
                return new Error("Process could not be shut down because it's not running.", ManagerErrorCode.ProcessNotRunning);
            }

            _logger.LogDebug("Shutting down the {ProcessType}: {ProcessName}", ProcessType, _process.ProcessName);

            _process.CloseMainWindow();
            await Task.Run(() => _process.WaitForExit(milliseconds: 5000));
            
            if (!_process.HasExited)
            {
                _logger.LogWarning("Process didn't stop gracefully. Killing process");
                _process.Kill();
            }
            
            _process = null;

            _logger.LogInformation("Server successfully shut down");

            return UnitResult.Success<IError>();
        }

        private void InvokeOnProcessShutdown()
        {
            _logger.LogDebug("{ProcessType} process shutdown detected. Invoking OnProcessShutdown event", ProcessType);
            OnProcessShutdown?.Invoke(this);
        }
    }
}
