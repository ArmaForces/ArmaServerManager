using System.Linq;
using Arma.Server.Config;
using Arma.Server.Extensions;
using Arma.Server.Mod;
using Arma.Server.Modset;

namespace Arma.Server.Providers.Parameters
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

        public string GetStartupParams()
            => string.Join(
                ' ',
                "-client",
                "-connect=127.0.0.1",
                $"-port={_port}",
                "-password=PLACEHOLDER", // TODO: Get password from server.cfg
                $"-profiles=\"{_modsetConfig.HCProfileDirectory}\"",
                "-limitFPS=100",
                GetModsStartupParam(_modset));

        internal static string GetModsStartupParam(IModset modset)
        {
            var mods = modset.Mods
                .Where(x => x.Type == ModType.ServerSide || x.Type == ModType.Required)
                .GetDirectories();

            return mods.Any()
                ? "-mod=" + string.Join(";", mods)
                : string.Empty;
        }
    }
}
