using System;
using System.Collections.Generic;

namespace Arma.Server.Modlist {
    public interface IModlist {
        string Id { get; }
        string Name { get; }
        DateTime CreatedAt { get; }
        DateTime? LastUpdatedAt { get; }
        List<Mod.Mod> Mods { get; }
    }
}