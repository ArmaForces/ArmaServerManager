using System;
using System.IO.Abstractions;

namespace ArmaForces.Arma.Server.Features.Mods {
    public interface IMod : IEquatable<IMod> {
        string WebId { get; }
        string Name { get; }
        DateTime CreatedAt { get; }
        DateTime? LastUpdatedAt { get; }
        ModSource Source { get; }
        ModType Type { get; }
        long WorkshopId { get; }
        string Directory { get; set; }

        bool Exists(IFileSystem fileSystem = null);
        
        new bool Equals(IMod mod);

        string ToShortString();
    }
}