using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using ArmaForces.Arma.Server.Config;
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

        public IMod CreateModFromDirectory(string directoryPath)
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

        public IMod TryEnsureModDirectory(IMod mod)
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

        public Result<string> TryFindModDirectory(IMod mod, string directoryToSearch)
            => TryFindModDirectoryByDirectory(mod, directoryToSearch)
                .OnFailureCompensate(() => TryFindModDirectoryByWorkshopId(mod, directoryToSearch))
                .OnFailureCompensate(() => TryFindModDirectoryByName(mod, directoryToSearch))
                .OnFailureCompensate(() => TryFindModDirectoryByNamePrefixedWithAtSign(mod, directoryToSearch))
                .OnFailureCompensate(() => TryFindCdlcDirectory(mod))
                .OnFailureCompensate(() => Result.Failure<string>($"Directory not found for {mod.ToShortString()}."));

        private Result<string> TryFindModDirectoryByDirectory(IMod mod, string directoryToSearch)
        {
            if (string.IsNullOrWhiteSpace(mod.Directory))
                return Result.Failure<string>("Mod directory attribute is empty.");

            var path = Path.Join(directoryToSearch, mod.Directory);
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("Mod directory not found using its directory.");
        }

        private Result<string> TryFindModDirectoryByWorkshopId(IMod mod, string directoryToSearch)
        {
            if (string.IsNullOrWhiteSpace(mod.WorkshopId.ToString()))
                return Result.Failure<string>("Workshop ID attribute is empty.");

            var path = Path.Join(directoryToSearch, mod.WorkshopId.ToString());
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("Mod directory not found using its workshopId.");
        }

        private Result<string> TryFindModDirectoryByName(IMod mod, string directoryToSearch)
        {
            if (string.IsNullOrWhiteSpace(mod.Name))
                return Result.Failure<string>("Mod name attribute is empty.");

            var path = Path.Join(directoryToSearch, mod.Name);
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("Mod directory not found using its name.");
        }

        private Result<string> TryFindModDirectoryByNamePrefixedWithAtSign(IMod mod, string directoryToSearch)
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

        private Result<string> TryFindCdlcDirectory(IMod maybeCdlc)
        {
            if (string.IsNullOrWhiteSpace(maybeCdlc.Directory))
                return Result.Failure<string>("cDLC directory attribute is empty.");

            // TODO: Parametrize directory to search
            var path = Path.Join(_serverPath, maybeCdlc.Directory);
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("cDLC directory not found.");
        }
    }
}
