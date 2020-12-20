using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Clients.Modsets;
using ArmaForces.ArmaServerManager.Features.Mods;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Providers
{
    public class ModsetProvider : IModsetProvider
    {
        private readonly IApiModsetClient _apiModsetClient;
        private readonly IModsCache _modsCache;

        public ModsetProvider(IApiModsetClient apiModsetClient, IModsCache modsCache)
        {
            _apiModsetClient = apiModsetClient;
            _modsCache = modsCache;
        }

        public Result<IModset> GetModsetByName(string modsetName)
        {
            var webModset = _apiModsetClient.GetModsetDataByName(modsetName);

            return Result.Success(_modsCache.MapWebModsetToCacheModset(webModset));
        }

        public Result<IEnumerable<IModset>> GetModsets()
        {
            var modsets = _apiModsetClient.GetModsets()
                .Select(x => _modsCache.MapWebModsetToCacheModset(x));

            return Result.Success(modsets);
        }
    }
}
