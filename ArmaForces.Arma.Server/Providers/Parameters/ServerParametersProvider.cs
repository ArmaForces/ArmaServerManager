using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Features.Modsets;

namespace ArmaForces.Arma.Server.Providers.Parameters
{
    public class ServerParametersProvider : IParametersProvider
    {
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

        public string GetStartupParams()
            => string.Join(
                ' ',
                $"-port={_port}",
                $"\"-config={_modsetConfig.ServerCfg}\"",
                $"\"-cfg={_modsetConfig.BasicCfg}\"",
                $"-profiles=\"{_modsetConfig.ServerProfileDirectory}\"",
                GetSpecialParams(),
                GetModsStartupParam(_modset));

        internal static string GetModsStartupParam(IModset modset) 
            => string.Join(
                ' ',
                GetServerModsStartupParam(modset),
                GetRequiredModsStartupParam(modset));

        private static string GetServerModsStartupParam(IModset modset)
        {
            var serverMods = modset.ServerSideMods
                .GetDirectories();

            return serverMods.Any()
                ? "-serverMod=" + string.Join(";", serverMods)
                : null;
        }

        private static string GetRequiredModsStartupParam(IModset modset)
        {
            var mods = modset.RequiredMods
                .GetDirectories();

            return mods.Any()
                ? "-mod=" + string.Join(";", mods)
                : null;
        }

        /// <summary>
        ///     TODO: Way to specify these
        /// </summary>
        /// <returns></returns>
        private static string GetSpecialParams()
            => string.Join(
                ' ',
                "-name=server",
                "-filePatching",
                "-netlog",
                "-limitFPS=100",
                "-loadMissionToMemory");
    }
}
