using System.Runtime.Serialization;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs {
    public enum WebModSource {
        [EnumMember(Value = "steam_workshop")]
        SteamWorkshop,

        [EnumMember(Value = "directory")]
        Directory
    }
}