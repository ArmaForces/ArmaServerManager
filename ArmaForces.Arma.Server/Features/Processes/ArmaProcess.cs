using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Parameters;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Processes
{
    public class ArmaProcess : IArmaProcess
    {
        private readonly string _arguments;
        private readonly string _executablePath;

        private readonly ILogger<ArmaProcess> _logger;

        private Process _serverProcess;

        public ArmaProcess(
            string executablePath,
            string arguments,
            ILogger<ArmaProcess> logger)
        {
            _executablePath = executablePath;
            _arguments = arguments;
            _logger = logger;
        }

        public ArmaProcess(
            Process process,
            ServerParameters serverParameters,
            ILogger<ArmaProcess> logger) : this(serverParameters, logger)
        {
            _serverProcess = process;
        }

        public ArmaProcess(
            ServerParameters serverParameters,
            ILogger<ArmaProcess> logger)
        {
            Parameters = serverParameters;
            _logger = logger;
        }

        public ServerParameters Parameters { get; }

        public ArmaProcessType ProcessType => Parameters.Client
            ? ArmaProcessType.HeadlessClient
            // TODO: Handle regular client process too to avoid shutting it down
            : ArmaProcessType.Server;

        public bool IsStopped => _serverProcess == null || _serverProcess.HasExited;

        public bool IsStartingOrStarted => !IsStopped;

        public event Func<IArmaProcess, Task> OnProcessShutdown;

        public Result Start()
        {
            if (!IsStopped) return Result.Success();

            try
            {
                // TODO: Use only ServerParameters to avoid issues
                //_armaProcess = Process.Start(Parameters.GetProcessStartInfo());
                _logger.LogTrace(
                    "Starting {executablePath} with {arguments}.",
                    _executablePath,
                    _arguments);
                _serverProcess = Process.Start(_executablePath, _arguments);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Arma 3 process could not be started.");
                return Result.Failure("Arma 3 process could not be started.");
            }

            _serverProcess!.Exited += async (sender, args) => await OnShutdown();

            _logger.LogInformation("Starting Arma 3 Server");

            return Result.Success();
        }

        public Result Shutdown()
        {
            if (IsStopped)
            {
                _logger.LogInformation("Server not running.");
                return Result.Failure("Server could not be shut down because it's not running.");
            }

            _logger.LogDebug("Shutting down the {armaProcess}.", _serverProcess);

            _serverProcess.Kill();
            _serverProcess = null;

            _logger.LogInformation("Server successfully shut down.");

            return Result.Success();
        }

        private async Task OnShutdown()
        {
            var handler = OnProcessShutdown;

            if (handler == null)
            {
                return;
            }

            var handlerTasks = handler.GetInvocationList()
                .Select(@delegate => ((Func<IArmaProcess, Task>) @delegate)(this));

            await Task.WhenAll(handlerTasks);
        }
    }
}
