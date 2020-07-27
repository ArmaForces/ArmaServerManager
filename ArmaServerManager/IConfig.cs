using CSharpFunctionalExtensions;

namespace ArmaServerManager {
    public interface IConfig {
        string GetConfigDir();

        Result LoadConfig();
    }
}