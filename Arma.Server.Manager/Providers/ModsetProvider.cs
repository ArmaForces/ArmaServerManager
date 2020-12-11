﻿using System.Collections.Generic;
using System.Linq;
using Arma.Server.Manager.Clients.Modsets;
using Arma.Server.Manager.Features.Mods;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Providers
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
