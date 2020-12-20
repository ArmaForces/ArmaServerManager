using System.Collections.Generic;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Clients.Modsets.Entities;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Mods {
    public interface IModsCache {

        /// <summary>
        ///     Checks if mod exists in mods directory.
        /// </summary>
        /// <param name="mod">Mod to check if it exists.</param>
        /// <returns>True if mod directory is found.</returns>
        Task<bool> ModExists(IMod mod);

        IModset MapWebModsetToCacheModset(WebModset webModset);

        /// <summary>
        /// All cached mods.
        /// </summary>
        public ISet<IMod> Mods { get; }

        /// <summary>
        ///     Saves cache to file.
        /// </summary>
        Task SaveCache();

        /// <summary>
        ///     Adds <paramref name="mod"/> to mods cache and saves it.
        /// </summary>
        /// <param name="mod"></param>
        Task<Result<List<IMod>>> AddOrUpdateCache(IEnumerable<IMod> mod);
    }
}