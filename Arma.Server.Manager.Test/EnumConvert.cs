using Newtonsoft.Json;

namespace Arma.Server.Manager.Test {
    public static class EnumConvert {
        public static string ToEnumString<T>(T value) {
            return JsonConvert.SerializeObject(value).Replace("\"", "");
        }

        public static T ToEnum<T>(string value) {
            return JsonConvert.DeserializeObject<T>($"\"{value}\"");
        }
    }
}