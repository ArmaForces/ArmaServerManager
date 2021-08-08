using System;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs
{
    public class ContentItem : IEquatable<ContentItem>
    {
        public string? Directory { get; set; }

        public uint Id { get; set; }

        public ItemType ItemType { get; set; }

        public override string ToString() => (ItemType == ItemType.Mod ? "Mod:" : "App:") + Id;

        public bool Equals(ContentItem? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Directory == other.Directory && Id == other.Id && ItemType == other.ItemType;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContentItem) obj);
        }

        public override int GetHashCode() => HashCode.Combine(
            Directory,
            Id,
            (int) ItemType);
    }
}
