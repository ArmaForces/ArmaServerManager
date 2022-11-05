using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ArmaForces.Arma.Server.Features.Dlcs;
using ArmaForces.Arma.Server.Features.Mods;

namespace ArmaForces.Arma.Server.Features.Modsets
{
    public class Modset : IEquatable<Modset>
    {
        /// <summary>
        /// Identifier of the modset in web system.
        /// </summary>
        public string? WebId { get; set; }

        /// <summary>
        /// Name of the modset.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Time when modset was last updated.
        /// </summary>
        public DateTime? LastUpdatedAt { get; set; }

        /// <summary>
        /// All mods contained in the modset.
        /// </summary>
        public ISet<Mod> Mods { get; set; } = new HashSet<Mod>();

        /// <summary>
        /// All dlcs required by the modset.
        /// </summary>
        public ISet<Dlc> Dlcs { get; set; } = new HashSet<Dlc>();

        /// <summary>
        /// All mods which are not disabled and can be loaded by server.
        /// </summary>
        public ISet<Mod> ActiveMods
            => Mods
                .Where(x => x.Status != ModStatus.Disabled)
                .ToHashSet();
        
        /// <summary>
        /// All mods which are required for client to load.
        /// </summary>
        public ISet<Mod> RequiredMods
            => Mods
                .Where(x => x.Type == ModType.Required)
                .Where(x => x.Status != ModStatus.Disabled)
                .Concat(Dlcs)
                .ToHashSet();

        /// <summary>
        /// All mods which are prohibited for client to load but must be loaded on the server.
        /// </summary>
        public ISet<Mod> ServerSideMods
            => Mods
                .Where(x => x.Type == ModType.ServerSide)
                .Where(x => x.Status != ModStatus.Disabled)
                .ToHashSet();

        /// <summary>
        /// All mods which are allowed for client to load but are not loaded on the server. 
        /// </summary>
        public ISet<Mod> ClientLoadableMods
            => Mods
                .Concat(Dlcs)
                .Where(x => x.Type > ModType.ServerSide)
                .Where(x => x.Status != ModStatus.Disabled)
                .ToHashSet();

        public bool Equals(Modset? modset)
        {
            if (modset is null) return false;
            if (ReferenceEquals(this, modset)) return true;
            return Name == modset.Name
                   && Mods.SetEquals(modset.Mods);
        }

        public override bool Equals(object? obj)
            => Equals(obj as Modset);

        // Disabled warning as properties cannot be readonly if we want to initialize them without constructor.
        // Modset name can change but then the local modset would be different and this is correct behavior.
        // TODO: Change Modset to be record
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
            => HashCode.Combine(WebId, Name, Mods, Dlcs);
    }
}