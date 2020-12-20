using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Tests {
    public static class EnumConvert {
        public static string ToEnumString<T>(T value) {
            return JsonConvert.SerializeObject(value).Replace("\"", "");
        }

        public static T ToEnum<T>(string value) {
            return JsonConvert.DeserializeObject<T>($"\"{value}\"");
        }
    }
}