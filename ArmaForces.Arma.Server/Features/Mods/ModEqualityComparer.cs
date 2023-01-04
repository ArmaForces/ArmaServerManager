using System.Collections.Generic;

namespace ArmaForces.Arma.Server.Features.Mods
{
    public class ModEqualityComparer : IEqualityComparer<Mod>
    {
        public bool Equals(Mod? x, Mod? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            return x.GetType() == y.GetType() && x.Equals(y);
        }

        public int GetHashCode(Mod obj)
        {
            return obj.GetHashCode();
        }
    }
}
