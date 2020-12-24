using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Mods;
using Microsoft.Extensions.Logging;

namespace ArmaForces.ArmaServerManager.Features.Mods
{
    public class ModDirectoryFinder : IModDirectoryFinder
    {
        private readonly ILogger<ModDirectoryFinder> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly string _modsPath;

        public ModDirectoryFinder(
            ISettings settings,
            ILogger<ModDirectoryFinder> logger,
            IFileSystem fileSystem = null)
        {
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
            mod.Directory = TryFindModDirectory(_modsPath, mod);
            return mod;
        }

        private string TryFindModDirectory(string modsDirectory, IMod mod)
        {
            string path;
            path = Path.Join(modsDirectory, mod.Directory);
            if (_fileSystem.Directory.Exists(path)) return path;
            path = Path.Join(modsDirectory, mod.WorkshopId.ToString());
            if (_fileSystem.Directory.Exists(path)) return path;
            path = Path.Join(modsDirectory, mod.Name);
            if (_fileSystem.Directory.Exists(path)) return path;
            path = Path.Join(
                modsDirectory,
                string.Join(
                    "",
                    "@",
                    mod.Name));
            if (_fileSystem.Directory.Exists(path)) return path;

            _logger.LogInformation("Directory not found for {mod}.", mod.ToString());

            return null;
        }
    }
}
