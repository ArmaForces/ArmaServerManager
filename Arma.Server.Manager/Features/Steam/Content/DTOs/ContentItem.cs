using BytexDigital.Steam.Core.Structs;

namespace Arma.Server.Manager.Features.Steam.Content.DTOs
{
    public class ContentItem
    {
        public string Directory { get; set; }

        public uint Id { get; set; }

        public ItemType ItemType { get; set; }

        public ManifestId? ManifestId { get; set; }

        public override string ToString() => (ItemType == ItemType.Mod ? "Mod:" : "App:") + Id;
    }
}
