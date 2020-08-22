using System;

namespace Arma.Server.Mod {
    public interface IMod {
        string Id { get; }
        string Name { get; }
        DateTime CreatedAt { get; }
        DateTime? LastUpdatedAt { get; }
        ModSource Source { get; }
        ModType Type { get; }
        int ItemId { get; }
        string Directory { get; }
    }
}