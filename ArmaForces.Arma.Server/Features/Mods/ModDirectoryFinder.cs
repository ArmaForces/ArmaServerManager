using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Exceptions;
using ArmaForces.Arma.Server.Features.Dlcs;
using ArmaForces.Arma.Server.Features.Dlcs.Constants;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.Arma.Server.Features.Mods
{
    internal class ModDirectoryFinder : IModDirectoryFinder
    {
        private readonly ILogger<ModDirectoryFinder> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly string _modsPath;
        private readonly string _serverPath;

        public ModDirectoryFinder(
            ISettings settings,
            ILogger<ModDirectoryFinder> logger,
            IFileSystem? fileSystem = null)
        {
            _serverPath = settings.ServerDirectory!;
            _modsPath = settings.ModsDirectory!;
            _fileSystem = fileSystem ?? new FileSystem();
            _logger = logger;
        }

        public Mod CreateModFromDirectory(string directoryPath)
        {
            var directoryName = directoryPath.Split('\\').Last();
            var mod = new Mod();
            try
            {
                mod.WorkshopId = long.Parse(directoryName);
                mod.Source = ModSource.SteamWorkshop;
            }
            catch (FormatException)
            {
                mod.Name = directoryName;
                mod.Source = ModSource.Directory;
            }

            mod.Directory = directoryPath;
            mod.CreatedAt = _fileSystem.Directory.GetLastWriteTimeUtc(directoryPath);
            mod.LastUpdatedAt = mod.CreatedAt;
            mod.Type = ModType.Required;

            return mod;
        }

        public Mod TryEnsureModDirectory(Mod mod)
        {
            if (mod.Exists(_fileSystem)) return mod;
            mod.Directory = TryFindModDirectory(mod, _modsPath)
                .Match(
                    onSuccess: directory => directory,
                    onFailure: error =>
                    {
                        _logger.LogInformation(error);
                        return null!;
                    });
            return mod;
        }

        public Result<string> TryFindModDirectory(Mod mod, string directoryToSearch)
            => TryFindModDirectoryByDirectory(mod, directoryToSearch)
                .OnFailureCompensate(() => TryFindModDirectoryByWorkshopId(mod, directoryToSearch))
                .OnFailureCompensate(() => TryFindModDirectoryByName(mod, directoryToSearch))
                .OnFailureCompensate(() => TryFindModDirectoryByNamePrefixedWithAtSign(mod, directoryToSearch))
                .OnFailureCompensate(() => TryFindCdlcDirectory(mod))
                .OnFailureCompensate(() => Result.Failure<string>($"Directory not found for {mod.ToShortString()}."))
                .Bind(directory => AssertFoundDirectoryIsNotServerDirectory(mod, directory));

        private Result<string> TryFindModDirectoryByDirectory(Mod mod, string directoryToSearch)
        {
            if (string.IsNullOrWhiteSpace(mod.Directory))
                return Result.Failure<string>("Mod directory attribute is empty.");

            var path = Path.Join(directoryToSearch, mod.Directory);
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("Mod directory not found using its directory.");
        }

        private Result<string> TryFindModDirectoryByWorkshopId(Mod mod, string directoryToSearch)
        {
            if (string.IsNullOrWhiteSpace(mod.WorkshopId.ToString()))
                return Result.Failure<string>("Workshop ID attribute is empty.");

            var path = Path.Join(directoryToSearch, mod.WorkshopId.ToString());
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("Mod directory not found using its workshopId.");
        }

        private Result<string> TryFindModDirectoryByName(Mod mod, string directoryToSearch)
        {
            if (string.IsNullOrWhiteSpace(mod.Name))
                return Result.Failure<string>("Mod name attribute is empty.");

            var path = Path.Join(directoryToSearch, mod.Name);
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("Mod directory not found using its name.");
        }

        private Result<string> TryFindModDirectoryByNamePrefixedWithAtSign(Mod mod, string directoryToSearch)
        {
            if (string.IsNullOrWhiteSpace(mod.Name))
                return Result.Failure<string>("Mod name attribute is empty.");

            var path = Path.Join(
                directoryToSearch,
                string.Join(
                    "",
                    "@",
                    mod.Name));
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("Mod directory not found using its name with @ prefix.");
        }

        private Result<string> TryFindCdlcDirectory(Mod maybeCdlc)
        {
            if (!(maybeCdlc is Dlc cdlc))
                return Result.Failure<string>("Mod is not a cDLC.");
            
            if (string.IsNullOrWhiteSpace(cdlc.Directory))
            {
                cdlc.Directory = DlcDirectoryName.GetName(cdlc.AppId);
            }

            // TODO: Parametrize directory to search
            var path = Path.Join(_serverPath, cdlc.Directory);
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("cDLC directory not found.");
        }

        /// <summary>
        /// Checks if <paramref name="foundDirectory"/> equals to main arma server directory and throws an exception then.
        /// This prevents any fuck ups in server directory.
        /// This is just a failsafe, it should never be called actually.
        /// Might be removed when 100% sure it won't happen again.
        /// </summary>
        /// <param name="mod">Mod for which the <paramref name="foundDirectory"/> was found.</param>
        /// <param name="foundDirectory">Directory path to mod.</param>
        /// <returns>Mod directory path.</returns>
        /// <exception cref="ModNotFoundException">Thrown when <paramref name="foundDirectory"/> equals to server directory.</exception>
        private Result<string> AssertFoundDirectoryIsNotServerDirectory(Mod mod, string foundDirectory)
        {
            if (foundDirectory != _serverPath)
            {
                return Result.Success(foundDirectory);
            }
            
            _logger.LogError("Directory for mod {@Mod} equals server directory while it should not!", mod);
            
            throw new ModNotFoundException(mod.ToShortString(), $"Directory for {mod.ToShortString()} equals server directory while it should not!");
        }
    }
}
