using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arma.Server.Config;
using Arma.Server.Mod;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;

namespace Arma.Server.Manager.Mods {
    public class ModsCache {
        private readonly ISet<IMod> _cache;
        private readonly string _cacheFilePath;
        private readonly string _modsPath;

        public ModsCache(ISettings settings) {
            _modsPath = settings.ModsDirectory;
            _cacheFilePath = $"{_modsPath}\\{settings.ModsManagerCacheFileName}.json";
            _cache = LoadCache()
                .OnFailureCompensate(x => BuildCache())
                .Value;
            SaveCache();
        }

        /// <summary>
        ///     Checks if mod exists in mods directory.
        /// </summary>
        /// <param name="mod">Mod to check if it exists.</param>
        /// <returns>True if mod directory is found.</returns>
        public bool ModExists(IMod mod) {
            IMod modInCache;
            string path;

            try {
                 modInCache = _cache.Single(cacheMod => cacheMod.Equals(mod));
            } catch (InvalidOperationException e) {
                path = TryFindModDirectory(_modsPath, mod);
                mod.Directory = path;
                _cache.Append(mod);
                return mod.Exists();
            }
            

            if (modInCache.Exists())
                return true;

            path = TryFindModDirectory(_modsPath, mod);
            modInCache.Directory = path;
            return modInCache.Exists();
        }

        private static string TryFindModDirectory(string modsDirectory, IMod mod) {
            string path;
            path = Path.Join(modsDirectory, mod.WorkshopId.ToString());
            if (Directory.Exists(path)) return path;
            path = Path.Join(modsDirectory, mod.Name);
            if (Directory.Exists(path)) return path;
            path = Path.Join(modsDirectory, string.Join("", "@", mod.Name));
            if (Directory.Exists(path)) return path;
            return null;
        }

        private Result<ISet<IMod>> LoadCache() {
            if (!File.Exists(_cacheFilePath)) return Result.Failure<ISet<IMod>>("Cache file does not exist.");
            var jsonString = File.ReadAllText(_cacheFilePath);
            var mods = JsonConvert.DeserializeObject<IEnumerable<Mod.Mod>>(jsonString)
                .Cast<IMod>()
                .ToHashSet();
            var cachedMods = RefreshCache(mods);
            return Result.Success(cachedMods);
        }

        private ISet<IMod> RefreshCache(ISet<IMod> mods) {
            mods = mods.Where(x => x.Exists())
                .ToHashSet();
            return mods;
        }

        private Result<ISet<IMod>> BuildCache() {
            var mods = (ISet<IMod>) Directory.GetDirectories(_modsPath)
                .Select(CreateModFromDirectory)
                .ToHashSet();
            return Result.Success(mods);
        }

        public void SaveCache() {
            SaveCache(_cache);
        }

        private void SaveCache(ISet<IMod> mods) {
            File.WriteAllText(_cacheFilePath, JsonConvert.SerializeObject(mods));
        }

        private IMod CreateModFromDirectory(string directoryPath) {
            var directoryName = directoryPath.Split('\\').Last();
            var mod = new Mod.Mod();
            try {
                mod.WorkshopId = int.Parse(directoryName);
                mod.Source = ModSource.SteamWorkshop;
            } catch (FormatException) {
                mod.Name = directoryName;
                mod.Source = ModSource.Directory;
            }

            mod.Directory = directoryPath;
            mod.CreatedAt = Directory.GetLastWriteTimeUtc(directoryPath);
            mod.LastUpdatedAt = mod.CreatedAt;
            mod.Type = ModType.Required;

            return mod;
        }
    }
}