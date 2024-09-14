using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArmaForces.Arma.Server.Constants;

public static class JsonOptions
{
    public static JsonSerializerOptions Legacy = new()
    {
        WriteIndented = true
    };
    
    public static JsonSerializerOptions Default = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };
}