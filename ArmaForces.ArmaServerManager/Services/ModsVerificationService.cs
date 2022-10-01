using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Services
{
    public class ModsVerificationService
    {
        private readonly IModsManager _modsManager;
        private readonly IModsetProvider _modsetProvider;

        public ModsVerificationService(IModsManager modsManager, IModsetProvider modsetProvider)
        {
            _modsManager = modsManager;
            _modsetProvider = modsetProvider;
        }

        public async Task<Result> VerifyModset(string modsetName, CancellationToken cancellationToken)
            => await _modsetProvider.GetModsetByName(modsetName)
                .Bind(x => VerifyModset(x, cancellationToken));

        private async Task<Result> VerifyModset(IModset modset, CancellationToken cancellationToken)
        {
            return await _modsManager.PrepareModset(modset, cancellationToken);
        }
    }
}
