using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Features.Mods
{
    public interface IModDirectoryFinder
    {
        Mod CreateModFromDirectory(string directoryPath);

        Mod TryEnsureModDirectory(Mod mod);

        Result<string> TryFindModDirectory(Mod mod, string directoryToSearch);
    }
}
