using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

namespace ArmaForces.Arma.Server.Features.Mods {
    public class Mod : IMod {
        public string WebId { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }
        
        public ulong? ManifestId { get; set; }

        public ModSource Source { get; set; }

        public ModType Type { get; set; }

        public long WorkshopId { get; set; }

        public string Directory { get; set; }

        public bool Exists(IFileSystem fileSystem = null) {
            fileSystem ??= new FileSystem();
            return !(Directory is null) 
                   && fileSystem.Directory.Exists(Directory);
        }

        public bool Equals(IMod mod) {
            if (mod is null) return false;
            if (ReferenceEquals(this, mod)) return true;
            return Source == ModSource.SteamWorkshop
                ? IsWorkshopModEqual(mod)
                : IsLocalModEqual(mod);
        }

        // Disabled warning as properties cannot be readonly if we want to initialize them without constructor.
        // Mod name can change but then the local mod would be different and this is correct behavior.
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return Source == ModSource.SteamWorkshop
                ? WorkshopId.GetHashCode()
                : Name.GetHashCode();
        }

        private bool IsWorkshopModEqual(IMod mod)
        {
            return Source == mod.Source && WorkshopId == mod.WorkshopId;
        }

        private bool IsLocalModEqual(IMod mod)
        {
            return Source == mod.Source && Name == mod.Name;
        }
    }
}