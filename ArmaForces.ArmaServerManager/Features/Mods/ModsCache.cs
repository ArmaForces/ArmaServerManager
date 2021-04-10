using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Features.Mods
{
    public class ModsCache : IModsCache, IWebModsetMapper
    {
        private readonly IModDirectoryFinder _modDirectoryFinder;
        private readonly IFileSystem _fileSystem;
        private readonly string _cacheFilePath;
        private readonly string _modsPath;

        private readonly ISet<IMod> _mods;

        // TODO: Make an asynchronous factory for it
        public ModsCache(
            ISettings settings,
            IModDirectoryFinder modDirectoryFinder,
            IFileSystem? fileSystem = null)
        {
            _modDirectoryFinder = modDirectoryFinder;
            _fileSystem = fileSystem ?? new FileSystem();
            _modsPath = settings.ModsDirectory!;
            _cacheFilePath = $"{_modsPath}\\{settings.ModsManagerCacheFileName}.json";

            // Blocking on asynchronous code as it's only done once at app startup
            _mods = LoadCache()
                .Result
                .OnFailureCompensate(x => BuildCacheFromModsDirectory())
                .Value;
            SaveCache().Wait();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IMod> Mods => _mods.ToList();

        /// <inheritdoc />
        public async Task<bool> ModExists(IMod mod)
        {
            var result = await GetModInCache(mod)
                .Bind(
                    x => x.Exists(_fileSystem)
                        ? Result.Success(x)
                        : Result.Failure<IMod>("Mod's directory doesn't exist."))
                .OnFailureCompensate(error => TryFindModInModsDirectory(mod))
                .Bind(AddOrUpdateModInCache)
                .Tap(SaveCache);

            return result.IsSuccess;
        }

        /// <inheritdoc />
        public IModset MapWebModsetToCacheModset(WebModset webModset)
        {
            var mappedMods = webModset.Mods
                .Select(MapWebModToCacheMod)
                .ToHashSet();

            return new Modset
            {
                Name = webModset.Name,
                WebId = webModset.Id,
                Mods = mappedMods
            };
        }

        /// <inheritdoc />
        public async Task SaveCache() 
            => await _fileSystem.File.WriteAllTextAsync(_cacheFilePath, JsonConvert.SerializeObject(_mods));

        /// <inheritdoc />
        public async Task<Result<List<IMod>>> AddOrUpdateModsInCache(IEnumerable<IMod> mods)
        {
            var cacheMods = mods.Select(x => AddOrUpdateModInCache(x).Value).ToList();
            await SaveCache();
            return Result.Success(cacheMods);
        }

        /// <summary>
        /// Gets given <paramref name="mod"/> from cache.
        /// </summary>
        /// <param name="mod">Mod to retrieve from cache.</param>
        /// <returns>Successful <see cref="Result"/> if mod was retrieved from cache, failure if it doesn't exist.</returns>
        private Result<IMod> GetModInCache(IMod mod)
        {
            var modInCache = _mods.SingleOrDefault(x => x.Equals(mod));

            return modInCache is null
                ? Result.Failure<IMod>("Mod doesn't exist in cache.")
                : Result.Success(modInCache);
        }

        /// <summary>
        /// Performs data update of existing <see cref="IMod"/> in cache, which corresponds to given <paramref name="mod"/>.
        /// The <see cref="IMod"/> corresponding to <paramref name="mod"/> must exist in cache.
        /// </summary>
        /// <param name="mod">Mod data to update corresponding mod in cache.</param>
        /// <returns>Successful <see cref="Result"/> if mod data was updated properly, failure if <see cref="IMod"/> doesn't exist in cache.</returns>
        private Result<IMod> UpdateModInCache(IMod mod)
        {
            return RemoveModFromCache(mod)
                .Bind(modInCache => modInCache.UpdateModData(mod))
                .Bind(AddModToCache);
        }

        /// <summary>
        /// Adds mod to cache.
        /// </summary>
        /// <param name="mod">Mod to be added to cache.</param>
        /// <returns>Successful <see cref="Result"/> if mod was added to cache, failure if it already exists.</returns>
        private Result<IMod> AddModToCache(IMod mod)
        {
            var result = _mods.Add(mod);

            return result
                ? Result.Success(mod)
                : Result.Failure<IMod>("Mod already exists in cache.");
        }

        /// <summary>
        /// Removes mod from cache.
        /// </summary>
        /// <param name="mod">Mod to remove from cache.</param>
        /// <returns>Successful <see cref="Result"/> if mod was removed from cache, failure if it doesn't exist in cache.</returns>
        private Result<IMod> RemoveModFromCache(IMod mod)
        {
            var result = _mods.Remove(mod);

            return result
                ? Result.Success(mod)
                : Result.Failure<IMod>("Mod couldn't be removed from cache as it doesn't exist.");
        }

        /// <summary>
        /// Attempts to map given <paramref name="webMod"/> to some cache <see cref="IMod"/>.
        /// On failure attempts to find <paramref name="webMod"/> directory and returns non-cache <see cref="IMod"/>.
        /// Directory property of the returned <see cref="IMod"/> might be null if it wasn't found.
        /// </summary>
        /// <param name="webMod">WebMod to map to cache mod.</param>
        /// <returns>Cache or non-cache <see cref="IMod"/>.</returns>
        private IMod MapWebModToCacheMod(WebMod webMod)
        {
            var convertedMod = webMod.ConvertForServer();
            var result = GetModInCache(convertedMod)
                .Bind(mod => mod.UpdateModData(convertedMod))
                .Bind(UpdateModInCache);

            return result.IsSuccess
                ? result.Value
                : _modDirectoryFinder.TryEnsureModDirectory(convertedMod);
        }

        /// <summary>
        /// Tries to add <paramref name="mod"/> to cache and if it already exists, updates it.
        /// </summary>
        /// <returns>Always successful <see cref="Result"/>.</returns>
        private Result<IMod> AddOrUpdateModInCache(IMod mod)
        {
            return AddModToCache(mod)
                .OnFailureCompensate(error => UpdateModInCache(mod));
        }

        /// <summary>
        /// Attempts to find given non-cached <paramref name="mod"/> in mods directory.
        /// If successful, mod will be added to cache.
        /// </summary>
        /// <param name="mod">The non-cached mod to look for in mods directory.</param>
        /// <returns>Successful <see cref="Result"/> if mod was found in the mods directory, failure if it couldn't be found.</returns>
        private Result<IMod> TryFindModInModsDirectory(IMod mod)
        {
            mod = _modDirectoryFinder.TryEnsureModDirectory(mod);

            return mod.Directory is null
                ? Result.Failure<IMod>("Mod directory could not be found.")
                : Result.Success(mod);
        }
        
        /// <summary>
        /// Performs cache loading from cache file.
        /// After loading the file performs quick filtering to exclude non-existing mods.
        /// </summary>
        /// <returns>Successful <see cref="Result"/> with cache mods.</returns>
        private async Task<Result<ISet<IMod>>> LoadCache()
        {
            if (!_fileSystem.File.Exists(_cacheFilePath))
                return Result.Failure<ISet<IMod>>("Cache file does not exist.");

            var jsonString = await _fileSystem.File.ReadAllTextAsync(_cacheFilePath);
            var mods = JsonConvert.DeserializeObject<IEnumerable<Mod>>(jsonString)
                .Cast<IMod>()
                .ToHashSet();
            var cachedMods = FilterOutNonExistingMods(mods);

            return Result.Success(cachedMods);
        }

        /// <summary>
        /// Filters out mods which don't exist.
        /// </summary>
        /// <param name="mods">Set of mods to filter.</param>
        /// <returns>Set of mods which exist.</returns>
        private ISet<IMod> FilterOutNonExistingMods(ISet<IMod> mods)
        {
            mods = mods.Where(x => x.Exists(_fileSystem))
                .ToHashSet();
            return mods;
        }

        /// <summary>
        /// Builds cache from mods directory, loading each folder as separate mod.
        /// </summary>
        /// <returns>Successful result with discovered mods.</returns>
        private Result<ISet<IMod>> BuildCacheFromModsDirectory()
        {
            var mods = _fileSystem.Directory.GetDirectories(_modsPath)
                .Select(_modDirectoryFinder.CreateModFromDirectory)
                .ToHashSet();
            return Result.Success<ISet<IMod>>(mods);
        }
    }
}
