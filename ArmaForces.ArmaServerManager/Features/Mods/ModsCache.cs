using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Dlcs;
using ArmaForces.Arma.Server.Features.Dlcs.Constants;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;

namespace ArmaForces.ArmaServerManager.Features.Mods
{
    internal class ModsCache : IModsCache, IWebModsetMapper
    {
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings{Formatting = Formatting.Indented};
        
        private readonly IModDirectoryFinder _modDirectoryFinder;
        private readonly IFileSystem _fileSystem;
        private readonly string _cacheFilePath;
        private readonly string _modsPath;
        private readonly string _armaPath;

        private readonly ISet<Mod> _mods;

        // TODO: Make an asynchronous factory for it
        public ModsCache(
            ISettings settings,
            IModDirectoryFinder modDirectoryFinder,
            IFileSystem? fileSystem = null)
        {
            _modDirectoryFinder = modDirectoryFinder;
            _fileSystem = fileSystem ?? new FileSystem();
            _modsPath = settings.ModsDirectory!;
            _armaPath = settings.ServerDirectory!;
            _cacheFilePath = $"{_modsPath}\\{settings.ModsManagerCacheFileName}.json";

            // Blocking on asynchronous code as it's only done once at app startup
            _mods = LoadCache()
                .Result
                .OnFailureCompensate(x => BuildCacheFromModsDirectory())
                .Value;
            
            SaveCache().Wait();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Mod> Mods => _mods.ToList();

        /// <inheritdoc />
        public async Task<bool> ModExists(Mod mod)
        {
            var result = await GetModInCache(mod)
                .Bind(
                    x => x.Exists(_fileSystem)
                        ? Result.Success(x)
                        : Result.Failure<Mod>("Mod's directory doesn't exist."))
                .OnFailureCompensate(error => TryFindModInModsDirectory(mod))
                .Bind(AddOrUpdateModInCache)
                .Tap(SaveCache);

            return result.IsSuccess;
        }

        /// <inheritdoc />
        public Modset MapWebModsetToCacheModset(WebModset webModset)
        {
            var mappedMods = webModset.Mods
                .Select(MapWebModToCacheMod)
                .ToHashSet();

            var mappedDlcs = webModset.Dlcs
                .Select(MapWebDlcToCacheDlc)
                .ToHashSet();

            return new Modset
            {
                Name = webModset.Name,
                WebId = webModset.Id,
                Mods = mappedMods,
                Dlcs = mappedDlcs
            };
        }

        /// <inheritdoc />
        public async Task<Result<List<Mod>>> AddOrUpdateModsInCache(IReadOnlyCollection<Mod> mods)
        {
            return await mods.Select(AddOrUpdateModInCache)
                .Combine()
                .Bind(x => Result.Success(x.ToList()))
                .Tap(async _ => await SaveCache());
        }

        /// <summary>
        /// Gets given <paramref name="mod"/> from cache.
        /// </summary>
        /// <param name="mod">Mod to retrieve from cache.</param>
        /// <returns>Successful <see cref="Result"/> if mod was retrieved from cache, failure if it doesn't exist.</returns>
        private Result<Mod> GetModInCache(Mod mod)
        {
            var modInCache = _mods.SingleOrDefault(x => x.Equals(mod));

            return modInCache is null
                ? Result.Failure<Mod>("Mod doesn't exist in cache.")
                : Result.Success(modInCache);
        }

        /// <summary>
        /// Performs data update of existing <see cref="Mod"/> in cache, which corresponds to given <paramref name="mod"/>.
        /// The <see cref="Mod"/> corresponding to <paramref name="mod"/> must exist in cache.
        /// </summary>
        /// <param name="mod">Mod data to update corresponding mod in cache.</param>
        /// <returns>Successful <see cref="Result"/> if mod data was updated properly, failure if <see cref="Mod"/> doesn't exist in cache.</returns>
        private Result<Mod> UpdateModInCache(Mod mod)
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
        private Result<Mod> AddModToCache(Mod mod)
        {
            var result = _mods.Add(mod);

            return result
                ? Result.Success(mod)
                : Result.Failure<Mod>("Mod already exists in cache.");
        }

        /// <summary>
        /// Removes mod from cache.
        /// </summary>
        /// <param name="mod">Mod to remove from cache.</param>
        /// <returns>Successful <see cref="Result"/> if mod was removed from cache, failure if it doesn't exist in cache.</returns>
        private Result<Mod> RemoveModFromCache(Mod mod)
        {
            var result = _mods.Remove(mod);

            return result
                ? Result.Success(mod)
                : Result.Failure<Mod>("Mod couldn't be removed from cache as it doesn't exist.");
        }

        /// <summary>
        /// Attempts to map given <paramref name="webMod"/> to some cache <see cref="Mod"/>.
        /// On failure attempts to find <paramref name="webMod"/> directory and returns non-cache <see cref="Mod"/>.
        /// Directory property of the returned <see cref="Mod"/> might be null if it wasn't found.
        /// </summary>
        /// <param name="webMod">WebMod to map to cache mod.</param>
        /// <returns>Cache or non-cache <see cref="Mod"/>.</returns>
        private Mod MapWebModToCacheMod(WebMod webMod)
        {
            var convertedMod = webMod.ConvertForServer();
            var result = GetModInCache(convertedMod)
                .Bind(mod => mod.UpdateModData(convertedMod))
                .Bind(UpdateModInCache);

            return result.IsSuccess
                ? result.Value
                : _modDirectoryFinder.TryEnsureModDirectory(convertedMod);
        }

        private Dlc MapWebDlcToCacheDlc(WebDlc webDlc)
        {
            return new Dlc
            {
                Name = webDlc.Name,
                WebId = webDlc.Id,
                CreatedAt = webDlc.CreatedAt,
                LastUpdatedAt = webDlc.LastUpdatedAt,
                AppId = (DlcAppId) webDlc.AppId,
                Directory = _fileSystem.Path.Join(_armaPath, webDlc.Directory ?? DlcDirectoryName.GetName((DlcAppId) webDlc.AppId))
            };
        }

        /// <summary>
        /// Tries to add <paramref name="mod"/> to cache and if it already exists, updates it.
        /// </summary>
        /// <returns>Always successful <see cref="Result"/>.</returns>
        private Result<Mod> AddOrUpdateModInCache(Mod mod)
        {
            return AddModToCache(mod)
                .OnFailureCompensate(_ => UpdateModInCache(mod));
        }

        /// <summary>
        /// Attempts to find given non-cached <paramref name="mod"/> in mods directory.
        /// If successful, mod will be added to cache.
        /// </summary>
        /// <param name="mod">The non-cached mod to look for in mods directory.</param>
        /// <returns>Successful <see cref="Result"/> if mod was found in the mods directory, failure if it couldn't be found.</returns>
        private Result<Mod> TryFindModInModsDirectory(Mod mod)
        {
            mod = _modDirectoryFinder.TryEnsureModDirectory(mod);

            return mod.Directory is null
                ? Result.Failure<Mod>("Mod directory could not be found.")
                : Result.Success(mod);
        }

        /// <summary>
        /// Performs cache loading from cache file.
        /// After loading the file performs quick filtering to exclude non-existing mods.
        /// </summary>
        /// <returns>Successful <see cref="Result"/> with cache mods.</returns>
        private async Task<Result<ISet<Mod>>> LoadCache()
        {
            if (!_fileSystem.File.Exists(_cacheFilePath))
                return Result.Failure<ISet<Mod>>("Cache file does not exist.");

            var jsonString = await _fileSystem.File.ReadAllTextAsync(_cacheFilePath);
            var mods = JsonConvert.DeserializeObject<IEnumerable<Mod>>(jsonString)?
                .ToHashSet() ?? new HashSet<Mod>();
            var cachedMods = FilterOutNonExistingMods(mods);

            return Result.Success(cachedMods);
        }

        /// <summary>
        /// Filters out mods which don't exist.
        /// </summary>
        /// <param name="mods">Set of mods to filter.</param>
        /// <returns>Set of mods which exist.</returns>
        private ISet<Mod> FilterOutNonExistingMods(ISet<Mod> mods)
            => mods.Where(x => x.Exists(_fileSystem))
                .ToHashSet();

        /// <summary>
        /// Builds cache from mods directory, loading each folder as separate mod.
        /// </summary>
        /// <returns>Successful result with discovered mods.</returns>
        private Result<ISet<Mod>> BuildCacheFromModsDirectory()
        {
            return _fileSystem.Directory.Exists(_modsPath)
                ? _fileSystem.Directory.GetDirectories(_modsPath)
                    .Select(_modDirectoryFinder.CreateModFromDirectory)
                    .ToHashSet()
                : new HashSet<Mod>();
        }

        /// <summary>
        /// Saves cache to file.
        /// </summary>
        private async Task SaveCache()
        {
            _fileSystem.Directory.CreateDirectory(_modsPath);
            
            await _fileSystem.File.WriteAllTextAsync(_cacheFilePath,
                JsonConvert.SerializeObject(_mods, _serializerSettings));
        }
    }
}
