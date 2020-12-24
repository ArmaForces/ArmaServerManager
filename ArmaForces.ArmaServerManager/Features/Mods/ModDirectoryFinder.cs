using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Mods;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Mods
{
    public class ModDirectoryFinder : IModDirectoryFinder
    {
        private readonly ILogger<ModDirectoryFinder> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly string _modsPath;
        private readonly string _serverPath;

        public ModDirectoryFinder(
            ISettings settings,
            ILogger<ModDirectoryFinder> logger,
            IFileSystem fileSystem = null)
        {
            _serverPath = settings.ServerDirectory;
            _modsPath = settings.ModsDirectory;
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
            mod.Directory = TryFindModDirectory(mod)
                .Match(
                    onSuccess: directory => directory,
                    onFailure: error =>
                    {
                        _logger.LogInformation(error);
                        return null;
                    });
            return mod;
        }

        private Result<string> TryFindModDirectory(IMod mod) 
            => TryFindModDirectoryByDirectory(mod)
                .OnFailureCompensate(() => TryFindModDirectoryByWorkshopId(mod))
                .OnFailureCompensate(() => TryFindModDirectoryByName(mod))
                .OnFailureCompensate(() => TryFindModDirectoryByNamePrefixedWithAtSign(mod))
                .OnFailureCompensate(() => TryFindCdlcDirectory(mod))
                .OnFailureCompensate(() => Result.Failure<string>($"Directory not found for {mod.ToShortString()}."));

        private Result<string> TryFindModDirectoryByDirectory(IMod mod)
        {
            var path = Path.Join(_modsPath, mod.Directory);
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("Mod directory not found using its directory.");
        }

        private Result<string> TryFindModDirectoryByWorkshopId(IMod mod)
        {
            var path = Path.Join(_modsPath, mod.WorkshopId.ToString());
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("Mod directory not found using its workshopId.");
        }

        private Result<string> TryFindModDirectoryByName(IMod mod)
        {
            var path = Path.Join(_modsPath, mod.Name);
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("Mod directory not found using its name.");
        }

        private Result<string> TryFindModDirectoryByNamePrefixedWithAtSign(IMod mod)
        {
            var path = Path.Join(
                _modsPath,
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
            var path = Path.Join(_serverPath, maybeCdlc.Directory);
            return _fileSystem.Directory.Exists(path)
                ? Result.Success(path)
                : Result.Failure<string>("cDLC directory not found.");
        }
    }
}
