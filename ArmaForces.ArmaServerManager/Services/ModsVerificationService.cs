using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Services
{
    /// <summary>
    /// Service allowing mods verification.
    /// </summary>
    public class ModsVerificationService : IModsVerificationService
    {
        private readonly IModsManager _modsManager;
        private readonly IModsetProvider _modsetProvider;

        public ModsVerificationService(IModsManager modsManager, IModsetProvider modsetProvider)
        {
            _modsManager = modsManager;
            _modsetProvider = modsetProvider;
        }

        /// <inheritdoc />
        public async Task<UnitResult<IError>> VerifyModset(string modsetName, CancellationToken cancellationToken)
            => await _modsetProvider.GetModsetByName(modsetName)
                .Bind(x => VerifyModset(x, cancellationToken));

        /// <inheritdoc />
        public async Task<UnitResult<IError>> VerifyModset(Modset modset, CancellationToken cancellationToken)
        {
            return await _modsManager.VerifyMods(modset.Mods.ToList(), cancellationToken);
        }
        
        /// <inheritdoc />
        public async Task<UnitResult<IError>> VerifyMods(IEnumerable<Mod> mods, CancellationToken cancellationToken)
        {
            return await _modsManager.VerifyMods(mods.ToList(), cancellationToken);
        }
    }
}