using System.Runtime.Serialization;

namespace ArmaForces.ArmaServerManager.Features.Modsets.DTOs {
    public enum WebModType {
        [EnumMember(Value = "server_side")]
        ServerSide,

        [EnumMember(Value = "required")]
        Required,

        [EnumMember(Value = "optional")]
        Optional,

        [EnumMember(Value = "client_side")]
        ClientSide
    }
}