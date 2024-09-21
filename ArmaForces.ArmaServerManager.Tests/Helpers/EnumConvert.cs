using System.Text.Json;
using ArmaForces.Arma.Server.Constants;

namespace ArmaForces.ArmaServerManager.Tests.Helpers {
    public static class EnumConvert {
        public static string ToEnumString<T>(T value) {
            return JsonSerializer.Serialize(value, JsonOptions.Default).Replace("\"", "");
        }

        public static T ToEnum<T>(string value)
        {
            return JsonSerializer.Deserialize<T>($"\"{value}\"", JsonOptions.Default);
        }
    }
}