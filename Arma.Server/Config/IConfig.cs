using CSharpFunctionalExtensions;

namespace Arma.Server.Config {
    public interface IConfig {
        string GetConfigDir();

        Result LoadConfig();
    }
}