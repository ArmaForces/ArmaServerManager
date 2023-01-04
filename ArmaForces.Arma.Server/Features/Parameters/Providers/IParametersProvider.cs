namespace ArmaForces.Arma.Server.Features.Parameters.Providers
{
    public interface IParametersProvider
    {
        ProcessParameters GetStartupParams(string exePath);
    }
}
