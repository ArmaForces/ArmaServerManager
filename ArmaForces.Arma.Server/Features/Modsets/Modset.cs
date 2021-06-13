using System;
using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Features.Dlcs;
using ArmaForces.Arma.Server.Features.Mods;

namespace ArmaForces.Arma.Server.Features.Modsets {
    public class Modset : IModset {
        public string? WebId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime? LastUpdatedAt { get; set; }

        public ISet<IMod> Mods { get; set; } = new HashSet<IMod>();

        public ISet<IDlc> Dlcs { get; set; } = new HashSet<IDlc>();

        public ISet<IMod> RequiredMods
            => Mods
                .Where(x => x.Type == ModType.Required)
                .ToHashSet();

        public ISet<IMod> ServerSideMods
            => Mods
                .Where(x => x.Type == ModType.ServerSide)
                .ToHashSet();

        public ISet<IMod> ClientLoadableMods
            => Mods
                .Where(x => x.Type > ModType.ServerSide)
                .ToHashSet();

        public bool Equals(IModset? modset)
        {
            if (modset is null) return false;
            if (ReferenceEquals(this, modset)) return true;
            return Name == modset.Name
                   && Mods.SetEquals(modset.Mods);
        }
    }
}