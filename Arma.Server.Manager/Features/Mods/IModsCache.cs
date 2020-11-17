using System.Collections.Generic;
using System.Threading.Tasks;
using Arma.Server.Mod;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Features.Mods {
    public interface IModsCache {

        /// <summary>
        ///     Checks if mod exists in mods directory.
        /// </summary>
        /// <param name="mod">Mod to check if it exists.</param>
        /// <returns>True if mod directory is found.</returns>
        Task<bool> ModExists(IMod mod);

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
        Task<Result<IEnumerable<IMod>>> AddOrUpdateCache(IEnumerable<IMod> mod);
    }
}