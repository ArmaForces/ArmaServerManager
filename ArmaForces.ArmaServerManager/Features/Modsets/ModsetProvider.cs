using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Modsets.Client;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Modsets
{
    internal class ModsetProvider : IModsetProvider
    {
        private readonly IApiModsetClient _apiModsetClient;
        private readonly IWebModsetMapper _webModsetMapper;

        public ModsetProvider(IApiModsetClient apiModsetClient, IWebModsetMapper webModsetMapper)
        {
            _apiModsetClient = apiModsetClient;
            _webModsetMapper = webModsetMapper;
        }

        public async Task<Result<Modset>> GetModsetByName(string modsetName)
        {
            return await _apiModsetClient.GetModsetDataByName(modsetName)
                .Bind(MapModsetData);
        }

        public async Task<Result<List<Modset>>> GetModsets()
        {
            return await _apiModsetClient.GetModsets()
                .Bind(MapModsetsData);
        }

        private Result<List<Modset>> MapModsetsData(List<WebModset> modsets)
            => modsets
                .Select(MapModsetData)
                .Combine()
                .Bind(x => Result.Success(x.ToList()));

        private Result<Modset> MapModsetData(WebModset modset) => Result.Success(_webModsetMapper.MapWebModsetToCacheModset(modset));
    }
}
