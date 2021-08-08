using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Features.Keys;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Processes;
using ArmaForces.Arma.Server.Features.Servers.Exceptions;
using ArmaForces.Arma.Server.Providers.Configuration;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Servers
{
    public class ServerBuilder : IServerBuilder
    {
        private readonly IKeysPreparer _keysPreparer;
        private readonly IModsetConfigurationProvider _modsetConfigurationProvider;
        private readonly IServerStatusFactory _serverStatusFactory;
        private readonly IArmaProcessManager _armaProcessManager;
        private readonly IArmaProcessFactory _armaProcessFactory;
        private readonly ILogger<ServerBuilder> _logger;
        private readonly ILogger<DedicatedServer> _dedicatedServerLogger;

        private int? _port;
        private int _numberOfHeadlessClients;

        private IModset? _modset;
        private IArmaProcess? _armaProcess;
        private IReadOnlyList<IArmaProcess>? _headlessClients;

        public ServerBuilder(
            IKeysPreparer keysPreparer,
            IModsetConfigurationProvider modsetConfigurationProvider,
            IServerStatusFactory serverStatusFactory,
            IArmaProcessManager armaProcessManager,
            IArmaProcessFactory armaProcessFactory,
            ILogger<ServerBuilder> logger,
            ILogger<DedicatedServer> dedicatedServerLogger)
        {
            _keysPreparer = keysPreparer;
            _modsetConfigurationProvider = modsetConfigurationProvider;
            _serverStatusFactory = serverStatusFactory;
            _armaProcessManager = armaProcessManager;
            _armaProcessFactory = armaProcessFactory;
            _logger = logger;
            _dedicatedServerLogger = dedicatedServerLogger;
        }

        public IServerBuilder OnPort(int port)
        {
            _port = port;
            return this;
        }

        public IServerBuilder WithModset(IModset modset)
        {
            _modset = modset;
            return this;
        }

        public IServerBuilder WithServerProcess(IArmaProcess armaProcess)
        {
            _armaProcess = armaProcess;
            return this;
        }

        public IServerBuilder WithHeadlessClients(IEnumerable<IArmaProcess>? headlessClients = null)
        {
            _headlessClients = headlessClients?.ToList();
            return this;
        }

        public IServerBuilder WithHeadlessClients(int numberOfHeadlessClients)
        {
            _numberOfHeadlessClients = numberOfHeadlessClients;
            return this;
        }

        public IDedicatedServer Build()
        {
            var validationResult = ValidateServerParameters();

            if (validationResult.IsFailure)
            {
                var exception = new ServerBuilderException(validationResult.Error); 
                _logger.LogError(exception, "Could not build dedicated server");
                throw exception;
            }

            var modsetConfig = _modsetConfigurationProvider.GetModsetConfig(_modset!.Name);

            var serverProcess = _armaProcess
                                ?? _armaProcessFactory.CreateServerProcess(
                                    _port!.Value,
                                    _modset,
                                    modsetConfig);

            var headlessClients = _headlessClients
                                  ?? _armaProcessFactory.CreateHeadlessClients(
                                      _port!.Value,
                                      _modset,
                                      modsetConfig,
                                      _numberOfHeadlessClients);

            return new DedicatedServer(
                _port!.Value,
                _modset,
                modsetConfig,
                _serverStatusFactory,
                _keysPreparer,
                _armaProcessManager,
                serverProcess,
                headlessClients,
                _dedicatedServerLogger);
        }

        private Result ValidateServerParameters()
        {
            if (_modset is null)
            {
                return Result.Failure("No modset specified.");
            }

            if (_port is null)
            {
                return Result.Failure("No port specified");
            }

            return Result.Success();
        }
    }
}
