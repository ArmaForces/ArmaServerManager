using System;
using System.Collections.Generic;
using ArmaForces.Arma.Server.Mod;

namespace ArmaForces.Arma.Server.Modset {
    public interface IModset {
        string WebId { get; }
        string Name { get; }
        DateTime? LastUpdatedAt { get; }
        ISet<IMod> Mods { get; }
        ISet<IMod> RequiredMods { get; }
        ISet<IMod> ServerSideMods { get; }
    }
}