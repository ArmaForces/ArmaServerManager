﻿using System;
using System.Diagnostics;
using ArmaForces.Arma.Server.Features.Parameters;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Server
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

        public ServerProcess(
            Process process,
            ServerParameters serverParameters,
            ILogger<ServerProcess> logger) : this(serverParameters, logger)
        {
            _serverProcess = process;
        }

        public ServerProcess(
            ServerParameters serverParameters,
            ILogger<ServerProcess> logger)
        {
            Parameters = serverParameters;
            _logger = logger;
        }

        public ServerParameters Parameters { get; }

        public bool IsStopped => _serverProcess == null || _serverProcess.HasExited;
        
        public bool IsStartingOrStarted => !IsStopped;

        public Result Start()
        {
            if (!IsStopped) return Result.Success();

            try
            {
                // TODO: Use only ServerParameters to avoid issues
                //_serverProcess = Process.Start(Parameters.GetProcessStartInfo());
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
            if (IsStopped)
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
