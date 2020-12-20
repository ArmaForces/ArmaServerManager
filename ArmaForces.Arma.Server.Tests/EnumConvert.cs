using Newtonsoft.Json;

namespace ArmaForces.Arma.Server.Tests {
    public static class EnumConvert {
        public static string ToEnumString<T>(T value) {
            return JsonConvert.SerializeObject(value).Replace("\"", "");
        }

        public static T ToEnum<T>(string value) {
            return JsonConvert.DeserializeObject<T>($"\"{value}\"");
        }
    }
}