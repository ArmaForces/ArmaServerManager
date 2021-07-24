using System.Collections.Generic;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Mods {
    public interface IModsCache {

        /// <summary>
        /// Checks if mod exists in mods directory.
        /// </summary>
        /// <param name="mod">Mod to check if it exists.</param>
        /// <returns>True if mod directory is found.</returns>
        Task<bool> ModExists(IMod mod);
        
        /// <summary>
        /// All cached mods.
        /// TODO: Change to IReadOnlySet after migration to newer .NET.
        /// </summary>
        public IReadOnlyCollection<IMod> Mods { get; }

        /// <summary>
        /// Adds <paramref name="mods"/> to mods cache or updates them if they already exists.
        /// </summary>
        /// <param name="mods">List of mods to be added or updated.</param>
        Task<Result<List<IMod>>> AddOrUpdateModsInCache(IReadOnlyCollection<IMod> mods);
    }
}