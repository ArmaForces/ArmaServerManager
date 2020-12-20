using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Mod;

namespace ArmaForces.Arma.Server.Extensions
{
    public static class ModEnumerableExtensions
    {
        public static List<string> GetDirectories(this IEnumerable<IMod> mods)
            => mods.Select(x => x.Directory).ToList();
    }
}
