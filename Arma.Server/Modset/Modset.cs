using System;
using System.Collections.Generic;
using System.Linq;
using Arma.Server.Mod;

namespace Arma.Server.Modset {
    public class Modset : IModset {
        public string WebId { get; set; }

        public string Name { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public ISet<IMod> Mods { get; set; }

        public ISet<IMod> RequiredMods
            => Mods
                .Where(x => x.Type == ModType.Required)
                .ToHashSet();

        public ISet<IMod> ServerSideMods
            => Mods
                .Where(x => x.Type == ModType.ServerSide)
                .ToHashSet();
    }
}