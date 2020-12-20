using System;
using System.Collections.Generic;
using ArmaForces.Arma.Server.Features.Mods;

namespace ArmaForces.Arma.Server.Features.Modsets {
    public interface IModset {
        string WebId { get; }
        string Name { get; }
        DateTime? LastUpdatedAt { get; }
        ISet<IMod> Mods { get; }
        ISet<IMod> RequiredMods { get; }
        ISet<IMod> ServerSideMods { get; }
    }
}