using System.Collections.Generic;
using System.Linq;
using Arma.Server.Mod;

namespace Arma.Server.Extensions
{
    public static class ModEnumerableExtensions
    {
        public static List<string> GetDirectories(this IEnumerable<IMod> mods)
            => mods.Select(x => x.Directory).ToList();
    }
}
