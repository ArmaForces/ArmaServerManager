using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Mods
{
    public interface IModDirectoryFinder
    {
        IMod CreateModFromDirectory(string directoryPath);

        IMod TryEnsureModDirectory(IMod mod);

        Result<string> TryFindModDirectory(IMod mod, string directoryToSearch);
    }
}
