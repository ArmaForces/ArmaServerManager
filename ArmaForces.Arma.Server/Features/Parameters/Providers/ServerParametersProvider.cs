using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Features.Modsets;

namespace ArmaForces.Arma.Server.Features.Parameters.Providers
{
    public class ServerParametersProvider : IParametersProvider
    {
        private const bool IsClient = false;
        
        private readonly int _port;
        private readonly IModset _modset;
        private readonly IModsetConfig _modsetConfig;

        public ServerParametersProvider(
            int port,
            IModset modset,
            IModsetConfig modsetConfig)
        {
            _port = port;
            _modset = modset;
            _modsetConfig = modsetConfig;
        }

        public ProcessParameters GetStartupParams(string exePath)
        {
            return new ProcessParameters(
                exePath,
                IsClient,
                _port,
                _modsetConfig.ServerCfg,
                _modsetConfig.BasicCfg,
                _modsetConfig.ServerProfileDirectory,
                "server",
                _modset.Name,
                filePatching: true,
                netLog: true,
                fpsLimit: 100,
                loadMissionToMemory: true,
                serverMods: _modset.ServerSideMods
                    .GetDirectories(),
                mods: _modset.RequiredMods
                    .GetDirectories());
        }
    }
}
