using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs {
    public enum WebModType {
        [EnumMember(Value = "server_side")]
        // TODO: Go back to it when .NET 9 comes out
        [JsonStringEnumMemberName("server_side")]
        ServerSide,

        [EnumMember(Value = "required")]
        Required,

        [EnumMember(Value = "optional")]
        Optional,

        [EnumMember(Value = "client_side")]
        // TODO: Go back to it when .NET 9 comes out
        [JsonStringEnumMemberName("client_side")]
        ClientSide
    }
}