using System.Collections.Generic;
using System.Diagnostics;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Parameters;
using ArmaForces.Arma.Server.Providers.Parameters;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Processes
{
    public class ArmaProcessFactory : IArmaProcessFactory
    {
        private readonly ISettings _settings;
        private readonly ILogger<ArmaProcess> _serverProcessLogger;

        public ArmaProcessFactory(
            ISettings settings,
            ILogger<ArmaProcess> serverProcessLogger)
        {
            _settings = settings;
            _serverProcessLogger = serverProcessLogger;
        }

        public IArmaProcess CreateServerProcess(Process process, ProcessParameters processParameters)
        {
            return new ArmaProcess(process, processParameters, _serverProcessLogger);
        }

        public IArmaProcess CreateServerProcess(
            int port,
            IModset modset,
            IModsetConfig modsetConfig)
        {
            var parametersProvider = new ServerParametersProvider(
                port,
                modset,
                modsetConfig);
            
            return new ArmaProcess(
                parametersProvider.GetStartupParams(_settings.ServerExecutable!),
                _serverProcessLogger);
        }

        public IEnumerable<IArmaProcess> CreateHeadlessClients(
            int port,
            IModset modset,
            IModsetConfig modsetConfig,
            int numberOfHeadlessClients = 1)
        {
            var parametersProvider = new HeadlessParametersProvider(
                port,
                modset,
                modsetConfig);

            for (var i = 0; i < numberOfHeadlessClients; i++)
            {
                yield return new ArmaProcess(
                    parametersProvider.GetStartupParams(_settings.ServerExecutable!),
                    _serverProcessLogger);
            }
        }
    }
}
