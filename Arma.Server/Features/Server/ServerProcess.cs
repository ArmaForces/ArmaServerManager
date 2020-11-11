﻿using System;
using System.Diagnostics;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Arma.Server.Features.Server
{
    public class ServerProcess : IServerProcess
    {
        private readonly string _arguments;
        private readonly string _executablePath;

        private readonly ILogger<ServerProcess> _logger;

        private Process _serverProcess;

        public ServerProcess(
            string executablePath,
            string arguments,
            ILogger<ServerProcess> logger)
        {
            _executablePath = executablePath;
            _arguments = arguments;
            _logger = logger;
        }

        public bool IsStopped => _serverProcess == null;

        public bool IsStarted => false; // TODO: Check if server is started, probably through named pipe

        public bool IsStarting => !IsStopped && !IsStarted;

        public Result Start()
        {
            try
            {
                _serverProcess = Process.Start(_executablePath, _arguments);
            }
            catch (InvalidOperationException exception)
            {
                _logger.LogError(exception, "Arma 3 Server could not be started. Path missing.");
                return Result.Failure("Arma 3 Server could not be started.");
            }

            _logger.LogInformation("Starting Arma 3 Server");

            return Result.Success();
        }

        public Result Shutdown()
        {
            if (_serverProcess == null)
            {
                _logger.LogInformation("Server not running.");
                return Result.Success();
            }

            _logger.LogDebug("Shutting down the {serverProcess}.", _serverProcess);

            _serverProcess.Kill();
            _serverProcess = null;

            _logger.LogInformation("Server successfully shut down.");

            return Result.Success();
        }
    }
}
