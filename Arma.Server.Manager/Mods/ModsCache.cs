﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Arma.Server.Config;
using Arma.Server.Mod;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Arma.Server.Manager.Mods
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
        public bool ModExists(IMod mod) => GetOrSetModInCache(mod).Exists(_fileSystem);

        /// <inheritdoc />
        public async Task SaveCache() => await SaveCache(Mods);

        public static ModsCache CreateModsCache(IServiceProvider serviceProvider)
            => new ModsCache(serviceProvider.GetService<ISettings>());

        private IMod GetOrSetModInCache(IMod mod)
        {
            try
            {
                var modInCache = Mods.Single(cacheMod => cacheMod.Equals(mod));
                return TryEnsureModDirectory(modInCache);
            } catch (InvalidOperationException)
            {
                mod = TryEnsureModDirectory(mod);
                Mods.Add(mod);
                return mod;
            }
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
            var mods = JsonConvert.DeserializeObject<IEnumerable<Mod.Mod>>(jsonString)
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
            var mod = new Mod.Mod();
            try
            {
                mod.WorkshopId = int.Parse(directoryName);
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
