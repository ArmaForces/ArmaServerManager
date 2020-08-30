using System.Runtime.Serialization;

namespace Arma.Server.Manager.Clients.Modsets.Entities {
    public enum WebModSource {
        [EnumMember(Value = "steam_workshop")]
        SteamWorkshop,

        [EnumMember(Value = "directory")]
        Directory
    }
}