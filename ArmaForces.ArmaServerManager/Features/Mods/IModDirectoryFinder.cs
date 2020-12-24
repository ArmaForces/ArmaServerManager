using ArmaForces.Arma.Server.Features.Mods;

namespace ArmaForces.ArmaServerManager.Features.Mods
{
    public interface IModDirectoryFinder
    {
        IMod CreateModFromDirectory(string directoryPath);

        IMod TryEnsureModDirectory(IMod mod);
    }
}
