namespace ArmaForces.Arma.Server.Features.Mods
{
    public interface IModDirectoryFinder
    {
        IMod CreateModFromDirectory(string directoryPath);

        IMod TryEnsureModDirectory(IMod mod);
    }
}
