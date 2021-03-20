using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Providers
{
    public class ModsetProvider : IModsetProvider
    {
        private readonly IApiModsetClient _apiModsetClient;
        private readonly IWebModsetMapper _webModsetMapper;

        public ModsetProvider(IApiModsetClient apiModsetClient, IWebModsetMapper webModsetMapper)
        {
            _apiModsetClient = apiModsetClient;
            _webModsetMapper = webModsetMapper;
        }

        public Result<IModset> GetModsetByName(string modsetName)
        {
            var webModset = _apiModsetClient.GetModsetDataByName(modsetName);

            return Result.Success(_webModsetMapper.MapWebModsetToCacheModset(webModset));
        }

        public Result<IEnumerable<IModset>> GetModsets()
        {
            var modsets = _apiModsetClient.GetModsets()
                .Select(x => _webModsetMapper.MapWebModsetToCacheModset(x));

            return Result.Success(modsets);
        }
    }
}
