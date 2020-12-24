using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Features.Mods
{
    public class ModsCache : IModsCache
    {
        private readonly string _cacheFilePath;
        private readonly IFileSystem _fileSystem;
        private readonly string _modsPath;

        public ModsCache(ISettings settings, IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? new FileSystem();
            _modsPath = settings.ModsDirectory;
            _cacheFilePath = $"{_modsPath}\\{settings.ModsManagerCacheFileName}.json";

            // Blocking on asynchronous code as it's only done once at app startup
            Mods = LoadCache()
                .Result
                .OnFailureCompensate(x => BuildCache())
                .Value;
            SaveCache().Wait();
        }

        /// <inheritdoc />
        public ISet<IMod> Mods { get; protected set; }

        /// <inheritdoc />
        public async Task<bool> ModExists(IMod mod)
        {
            mod = await GetOrSetModInCache(mod);
            return mod?.Exists(_fileSystem) ?? false;
        }

        public IModset MapWebModsetToCacheModset(WebModset webModset)
        {
            var mappedMods = webModset.Mods
                .Select(x => GetModInCache(x.ConvertForServer()))
                .ToHashSet();

            return new Modset
            {
                Name = webModset.Name,
                WebId = webModset.Id,
                Mods = mappedMods
            };
        }

        /// <inheritdoc />
        public async Task SaveCache() => await SaveCache(Mods);

        private IMod GetModInCache(IMod mod)
        {
            try
            {
                var modInCache = Mods.Single(x => x.Equals(mod));
                return TryEnsureModDirectory(modInCache);
            }
            catch (InvalidOperationException )
            {
                return TryEnsureModDirectory(mod);
            }
        }

        private async Task<IMod> GetOrSetModInCache(IMod mod)
        {
            try
            {
                var modInCache = Mods.Single(cacheMod => cacheMod.Equals(mod));
                return TryEnsureModDirectory(modInCache);
            } catch (InvalidOperationException)
            {
                mod = TryEnsureModDirectory(mod);
                return mod.Directory is null
                    ? null
                    : await AddModToCacheAndSave(mod);
            }
        }

        private async Task<IMod> AddModToCacheAndSave(IMod mod)
        {
            var modInCache = AddModToCache(mod);
            await SaveCache();
            return modInCache;
        }

        private IMod AddOrUpdateModInCache(IMod mod)
        {
            var modAddedToCache = Mods.Add(mod);

            return modAddedToCache
                ? mod
                : UpdateModInCache(mod);
        }

        private IMod AddModToCache(IMod mod)
        {
            Mods.Add(mod);
            return mod;
        }

        private IMod UpdateModInCache(IMod mod)
        {
            var modInCache = Mods.Single(x => x.Equals(mod));

            Mods.Remove(modInCache);
            var newMod = new Mod
            {
                Directory = mod.Directory ?? modInCache.Directory,
                CreatedAt = modInCache.CreatedAt,
                LastUpdatedAt = DateTime.Now,
                ManifestId = mod.ManifestId ?? modInCache.ManifestId,
                Name = mod.Name,
                Source = mod.Source,
                Type = mod.Type,
                WebId = mod.WebId ?? modInCache.WebId,
                WorkshopId = mod.WorkshopId
            };

            return AddModToCache(newMod);
        }

        public async Task<Result<List<IMod>>> AddOrUpdateCache(IEnumerable<IMod> mods)
        {
            var cacheMods = mods.Select(AddOrUpdateModInCache).ToList();
            await SaveCache();
            return Result.Success(cacheMods);
        }

        private IMod TryEnsureModDirectory(IMod mod)
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
            return null;
        }

        private async Task<Result<ISet<IMod>>> LoadCache()
        {
            if (!_fileSystem.File.Exists(_cacheFilePath))
                return Result.Failure<ISet<IMod>>("Cache file does not exist.");

            var jsonString = await _fileSystem.File.ReadAllTextAsync(_cacheFilePath);
            var mods = JsonConvert.DeserializeObject<IEnumerable<Mod>>(jsonString)
                .Cast<IMod>()
                .ToHashSet();
            var cachedMods = RefreshCache(mods);
            return Result.Success(cachedMods);
        }

        private ISet<IMod> RefreshCache(ISet<IMod> mods)
        {
            mods = mods.Where(x => x.Exists(_fileSystem))
                .ToHashSet();
            return mods;
        }

        private Result<ISet<IMod>> BuildCache()
        {
            var mods = (ISet<IMod>) _fileSystem.Directory.GetDirectories(_modsPath)
                .Select(CreateModFromDirectory)
                .ToHashSet();
            return Result.Success(mods);
        }

        private async Task SaveCache(ISet<IMod> mods)
            => await _fileSystem.File.WriteAllTextAsync(_cacheFilePath, JsonConvert.SerializeObject(mods));

        private IMod CreateModFromDirectory(string directoryPath)
        {
            var directoryName = directoryPath.Split('\\').Last();
            var mod = new Mod();
            try
            {
                mod.WorkshopId = long.Parse(directoryName);
                mod.Source = ModSource.SteamWorkshop;
            } catch (FormatException)
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
    }
}
