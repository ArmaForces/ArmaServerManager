using System;
using System.Collections.Generic;

namespace Arma.Server.Modset {
    public interface IModset {
        string Id { get; }
        string Name { get; }
        DateTime CreatedAt { get; }
        DateTime? LastUpdatedAt { get; }
        List<Mod.Mod> Mods { get; }
    }
}