using System.Linq;
using Arma.Server.Config;
using Arma.Server.Mod;
using Arma.Server.Modset;

namespace Arma.Server.Providers
{
    public class ParametersProvider : IParametersProvider
    {
        private readonly int _port;
        private readonly IModset _modset;
        private readonly IModsetConfig _modsetConfig;

        public ParametersProvider(
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

        private static string GetModsStartupParam(IModset modset)
            => false // TODO: Check here for HC in the future
                ? GetHcModsStartupParam(modset)
                : GetServerModsStartupParam(modset);

        private static string GetServerModsStartupParam(IModset modset)
        {
            var serverMods = modset.Mods
                .Where(x => x.Type.Equals(ModType.ServerSide))
                .Select(x => x.Directory);

            var mods = modset.Mods
                .Where(x => x.Type.Equals(ModType.Required))
                .Select(x => x.Directory);

            return "-serverMod=" + string.Join(";", serverMods) + " -mod=" + string.Join(";", mods);
        }

        private static string GetHcModsStartupParam(IModset modset)
        {
            var mods = modset.Mods
                .Where(x => x.Type.Equals(ModType.ServerSide) || x.Type.Equals(ModType.Required))
                .Select(x => x.Directory);

            return "-mod=" + string.Join(";", mods);
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
