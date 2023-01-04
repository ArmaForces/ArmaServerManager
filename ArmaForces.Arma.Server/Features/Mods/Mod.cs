using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

namespace ArmaForces.Arma.Server.Features.Mods
{
    public class Mod : IEquatable<Mod>
    {
        public string? WebId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }
        
        public virtual ModSource Source { get; set; }

        public ModStatus Status { get; set; }

        public virtual ModType Type { get; set; }

        public virtual long? WorkshopId { get; set; }

        public string? Directory { get; set; }

        public bool Exists(IFileSystem? fileSystem = null)
        {
            fileSystem ??= new FileSystem();
            return !(Directory is null) 
                   && fileSystem.Directory.Exists(Directory);
        }

        public bool Equals(Mod? mod)
        {
            if (mod is null) return false;
            if (ReferenceEquals(this, mod)) return true;
            return Source == ModSource.SteamWorkshop
                ? IsWorkshopModEqual(mod)
                : IsLocalModEqual(mod);
        }

        // Disabled warning as properties cannot be readonly if we want to initialize them without constructor.
        // Mod name can change but then the local mod would be different and this is correct behavior.
        // TODO: Change Mod to be record
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
            => Source == ModSource.SteamWorkshop
                ? WorkshopId.GetHashCode()
                : Name.GetHashCode();

        public string ToShortString()
            => Source == ModSource.SteamWorkshop
                ? $"{Name}:{WorkshopId}"
                : $"{Name}:{Directory}";

        private bool IsWorkshopModEqual(Mod mod)
            => Source == mod.Source && WorkshopId == mod.WorkshopId;

        private bool IsLocalModEqual(Mod mod)
            => Source == mod.Source && Name == mod.Name;

        public override bool Equals(object? obj)
            => Equals(obj as Mod);
    }
}