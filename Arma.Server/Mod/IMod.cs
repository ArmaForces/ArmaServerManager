using System;
using System.IO.Abstractions;

namespace Arma.Server.Mod {
    public interface IMod : IEquatable<IMod> {
        string WebId { get; }
        string Name { get; }
        DateTime CreatedAt { get; }
        DateTime? LastUpdatedAt { get; }
        ModSource Source { get; }
        ModType Type { get; }
        int WorkshopId { get; }
        string Directory { get; set; }

        bool Exists(IFileSystem fileSystem = null);
        new bool Equals(IMod mod);
    }
}