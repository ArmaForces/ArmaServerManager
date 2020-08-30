using System;
using System.Collections.Generic;

namespace Arma.Server.Modset {
    public interface IModset {
        string WebId { get; }
        string Name { get; }
        DateTime? LastUpdatedAt { get; }
        List<Mod.Mod> Mods { get; }
    }
}