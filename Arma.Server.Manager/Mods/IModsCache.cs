using System.Collections.Generic;
using System.Threading.Tasks;
using Arma.Server.Mod;

namespace Arma.Server.Manager.Mods {
    public interface IModsCache {

        /// <summary>
        ///     Checks if mod exists in mods directory.
        /// </summary>
        /// <param name="mod">Mod to check if it exists.</param>
        /// <returns>True if mod directory is found.</returns>
        bool ModExists(IMod mod);

        /// <summary>
        /// All cached mods.
        /// </summary>
        public ISet<IMod> Mods { get; }

        /// <summary>
        ///     Saves cache to file.
        /// </summary>
        Task SaveCache();
    }
}