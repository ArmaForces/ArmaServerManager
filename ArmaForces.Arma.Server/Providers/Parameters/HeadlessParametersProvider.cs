using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Constants;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Features.Parameters;

namespace ArmaForces.Arma.Server.Providers.Parameters
{
    public class HeadlessParametersProvider : IParametersProvider
    {
        private readonly int _port;
        private readonly IModset _modset;
        private readonly IModsetConfig _modsetConfig;

        public HeadlessParametersProvider(
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
                client: true,
                _port,
                _modsetConfig.ServerCfg,
                _modsetConfig.BasicCfg,
                _modsetConfig.HCProfileDirectory,
                "HC",
                _modset.Name,
                fpsLimit: 100,
                connectIpAddress: ParametersDefaults.ConnectIpAddress,
                connectPassword: _modsetConfig.ServerPassword,
                mods: _modset.ServerSideMods
                    .Concat(_modset.RequiredMods)
                    .GetDirectories());
        }
    }
}
