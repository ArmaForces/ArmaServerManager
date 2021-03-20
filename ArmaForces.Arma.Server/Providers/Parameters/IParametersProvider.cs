using ArmaForces.Arma.Server.Features.Parameters;

namespace ArmaForces.Arma.Server.Providers.Parameters
{
    public interface IParametersProvider
    {
        ProcessParameters GetStartupParams(string exePath);
    }
}
