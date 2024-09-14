using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs {
    public enum WebModSource {
        [EnumMember(Value = "steam_workshop")]
        // TODO: Go back to it when .NET 9 comes out
        [JsonStringEnumMemberName("steam_workshop")]
        SteamWorkshop,

        [EnumMember(Value = "directory")]
        Directory
    }
}