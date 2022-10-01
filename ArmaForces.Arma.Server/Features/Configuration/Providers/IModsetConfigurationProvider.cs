using ArmaForces.Arma.Server.Config;

namespace ArmaForces.Arma.Server.Features.Configuration.Providers
{
    public interface IModsetConfigurationProvider
    {
        IModsetConfig GetModsetConfig(string modsetName);
    }
}
