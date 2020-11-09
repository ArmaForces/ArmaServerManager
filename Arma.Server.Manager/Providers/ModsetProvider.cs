using System;
using System.Collections.Generic;
using System.Linq;
using Arma.Server.Manager.Clients.Modsets;
using Arma.Server.Mod;
using Arma.Server.Modset;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace Arma.Server.Manager.Providers
{
    public class ModsetProvider : IModsetProvider
    {
        private readonly IApiModsetClient _apiModsetClient;

        public ModsetProvider(IApiModsetClient apiModsetClient)
        {
            _apiModsetClient = apiModsetClient;
        }

        public Result<IModset> GetModsetByName(string modsetName)
        {
            var webModset = _apiModsetClient.GetModsetDataByName(modsetName);

            return Result.Success((IModset) webModset.ConvertForServer());
        }

        public Result<IEnumerable<IModset>> GetModsets()
        {
            var modsets = _apiModsetClient.GetModsets()
                .Select(x => (IModset) x.ConvertForServer());

            return Result.Success(modsets);
        }

        public static ModsetProvider CreateModsetProvider(IServiceProvider serviceProvider)
            => new ModsetProvider(serviceProvider.GetService<IApiModsetClient>());
    }
}
