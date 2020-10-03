using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;

namespace Arma.Server.Mod {
    public class Mod : IMod {
        public string WebId { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public ModSource Source { get; set; }

        public ModType Type { get; set; }

        public int WorkshopId { get; set; }

        public string Directory { get; set; }

        public bool Exists(IFileSystem fileSystem = null) {
            fileSystem ??= new FileSystem();
            return !(Directory is null) 
                   && fileSystem.Directory.Exists(Directory);
        }

        public bool Equals(IMod mod) {
            if (mod is null) return false;
            if (ReferenceEquals(this, mod)) return true;
            return Source == mod.Source && (WorkshopId == mod.WorkshopId || Name == mod.Name);
        }
    }
}