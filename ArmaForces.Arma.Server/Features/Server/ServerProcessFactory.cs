using System.Collections.Generic;
using System.Diagnostics;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Parameters;
using ArmaForces.Arma.Server.Providers.Parameters;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Server
{
    public class ServerProcessFactory : IServerProcessFactory
    {
        private readonly ISettings _settings;
        private readonly ILogger<ServerProcess> _serverProcessLogger;

        public ServerProcessFactory(
            ISettings settings,
            ILogger<ServerProcess> serverProcessLogger)
        {
            _settings = settings;
            _serverProcessLogger = serverProcessLogger;
        }

        public IServerProcess CreateServerProcess(Process process, ServerParameters serverParameters = null)
        {
            return new ServerProcess(process, serverParameters, _serverProcessLogger);
        }

        public IServerProcess CreateServerProcess(
            int port,
            IModset modset,
            IModsetConfig modsetConfig)
        {
            var parametersProvider = new ServerParametersProvider(
                port,
                modset,
                modsetConfig);

            return new ServerProcess(
                _settings.ServerExecutable,
                parametersProvider.GetStartupParams(),
                _serverProcessLogger);
        }

        public IEnumerable<IServerProcess> CreateHeadlessClients(
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
                yield return new ServerProcess(
                    _settings.ServerExecutable,
                    parametersProvider.GetStartupParams(),
                    _serverProcessLogger);
            }
        }
    }
}
