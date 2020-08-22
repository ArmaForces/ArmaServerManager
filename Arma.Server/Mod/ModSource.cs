using System.Runtime.Serialization;

namespace Arma.Server.Mod {
    public enum ModSource {
        [EnumMember(Value = "steam_workshop")]
        SteamWorkshop,

        [EnumMember(Value = "directory")]
        Directory
    }
}