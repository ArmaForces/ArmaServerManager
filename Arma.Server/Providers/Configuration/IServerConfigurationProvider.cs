using Arma.Server.Config;

namespace Arma.Server.Providers.Configuration
{
    public interface IServerConfigurationProvider
    {
        IModsetConfig GetModsetConfig(string modsetName);
    }
}
