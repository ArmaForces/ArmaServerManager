using System;
using System.Diagnostics;
using Arma.Server.Config;
using Arma.Server.Providers;
using Microsoft.Extensions.Logging;

namespace Arma.Server
{
    public class Server
    {
        private readonly ILogger<Server> _logger;
        private readonly IParametersProvider _parametersProvider;
        private readonly ISettings _settings;

        private Process _serverProcess;

        public Server(
            ISettings settings,
            IParametersProvider parametersProvider,
            ILogger<Server> logger)
        {
            _settings = settings;
            _logger = logger;
            _parametersProvider = parametersProvider;
        }

        public bool IsServerRunning() => _serverProcess != null;

        public bool Start()
        {
            _logger.LogInformation("Starting Arma 3 Server");
            try
            {
                _serverProcess = Process.Start(_settings.ServerExecutable, _parametersProvider.GetStartupParams());
            }
            catch (NullReferenceException exception)
            {
                _logger.LogError(exception, "Arma 3 Server could not be started. Path missing.");
                return false;
            }

            return true;
        }

        public void WaitUntilStarted()
        {
            _logger.LogInformation("Waiting for {serverProcess} to finish startup.", _serverProcess);
            _serverProcess.WaitForInputIdle();
        }

        public void Shutdown()
        {
            if (_serverProcess == null)
                return;
            _logger.LogInformation("Shutting down the {serverProcess}.", _serverProcess);
            _serverProcess.Kill();
            _serverProcess = null;
        }
    }
}
