using System.Runtime.Serialization;

namespace Arma.Server.Manager.Clients.Modsets.Entities {
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