using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Extensions;
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

        public async Task<Result<Modset, IError>> GetModsetByName(string modsetName)
        {
            return await _apiModsetClient.GetModsetDataByName(modsetName)
                .Bind(MapModsetData);
        }

        public async Task<Result<List<Modset>, IError>> GetModsets()
        {
            return await _apiModsetClient.GetModsets()
                .Bind(MapModsetsData);
        }

        private Result<List<Modset>, IError> MapModsetsData(List<WebModset> modsets)
            => modsets
                .Select(MapModsetData)
                .Combine()
                .Bind(x => x.ToList().ToResult());

        private Result<Modset, IError> MapModsetData(WebModset modset) => _webModsetMapper.MapWebModsetToCacheModset(modset);
    }
}
