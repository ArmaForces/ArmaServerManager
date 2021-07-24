using ArmaForces.Arma.Server.Config;

namespace ArmaForces.Arma.Server.Providers.Configuration
{
    public interface IModsetConfigurationProvider
    {
        IModsetConfig GetModsetConfig(string modsetName);
    }
}
