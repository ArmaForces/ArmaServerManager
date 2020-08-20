using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Arma.Server.Mod {
    public enum ModType {
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