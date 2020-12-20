using System.Runtime.Serialization;

namespace ArmaForces.ArmaServerManager.Clients.Modsets.Entities {
    public enum WebModSource {
        [EnumMember(Value = "steam_workshop")]
        SteamWorkshop,

        [EnumMember(Value = "directory")]
        Directory
    }
}