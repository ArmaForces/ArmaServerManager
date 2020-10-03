using System;
using System.Collections.Generic;
using Arma.Server.Mod;

namespace Arma.Server.Modset {
    public interface IModset {
        string WebId { get; }
        string Name { get; }
        DateTime? LastUpdatedAt { get; }
        ISet<IMod> Mods { get; }
    }
}