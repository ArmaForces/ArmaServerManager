using ArmaForces.Arma.Server.Config;

namespace ArmaForces.Arma.Server.Providers.Configuration
{
    public interface IServerConfigurationProvider
    {
        IModsetConfig GetModsetConfig(string modsetName);
    }
}
