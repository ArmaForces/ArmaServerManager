using Arma.Server.Manager.Mods;
using Arma.Server.Mod;
using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Arma.Server.Config;
using Moq;
using Newtonsoft.Json;
using Xunit;
using System.IO.Abstractions.TestingHelpers;

namespace Arma.Server.Manager.Test.Mods {
    public class ModsCacheTests: IDisposable {
        private readonly Fixture _fixture = new Fixture();
        private readonly string _workingDirectory = Path.Join(Directory.GetCurrentDirectory(), "mods");
        private readonly Mock<ISettings> _settingsMock = new Mock<ISettings>();
        private readonly IFileSystem _fileSystemMock = new MockFileSystem();
        private readonly IMod _mod;

        public ModsCacheTests() {
            _fileSystemMock.Directory.CreateDirectory(_workingDirectory);
            _settingsMock.Setup(x => x.ModsDirectory).Returns(_workingDirectory);
            _settingsMock.Setup(x => x.ModsManagerCacheFileName).Returns(".ManagerModsCache");
            _mod = FixtureCreateMod();
        }

        [Fact]
        public void ModExists_DirectoryNotExists_ReturnsFalse() {
            var modsCache = InitializeModsCache();
            modsCache.ModExists(_mod).Should().BeFalse();
        }

        [Fact]
        public void ModExists_DirectoryNamedIdExists_ReturnsTrue() {
            var modsCache = InitializeModsCache();
            var modDirectory = Path.Join(_workingDirectory, _mod.WorkshopId.ToString());
            _fileSystemMock.Directory.CreateDirectory(modDirectory);

            modsCache.ModExists(_mod).Should().BeTrue();
        }

        [Fact]
        public void ModExists_DirectoryNamedNameExists_ReturnsTrue() {
            var modsCache = InitializeModsCache();
            var modDirectory = Path.Join(_workingDirectory, _mod.Name);
            _fileSystemMock.Directory.CreateDirectory(modDirectory);

            modsCache.ModExists(_mod).Should().BeTrue();
        }

        [Fact]
        public void ModExists_DirectoryNamedWithAtExists_ReturnsTrue() {
            var modsCache = InitializeModsCache();
            var modDirectory = Path.Join(_workingDirectory, string.Join("", "@", _mod.Name));
            _fileSystemMock.Directory.CreateDirectory(modDirectory);

            modsCache.ModExists(_mod).Should().BeTrue();
        }

        [Fact]
        public void ModExists_CacheInvalidDirectoryRenamed_ReturnsTrue() {
            var oldModDirectory = Path.Join(_workingDirectory, _mod.WorkshopId.ToString());
            var newModDirectory = Path.Join(_workingDirectory, _mod.Name);
            _fileSystemMock.Directory.CreateDirectory(oldModDirectory);

            var modsCache = InitializeModsCache();
            _fileSystemMock.Directory.Delete(oldModDirectory);
            _fileSystemMock.Directory.CreateDirectory(newModDirectory);

            modsCache.ModExists(_mod).Should().BeTrue();
        }

        [Fact]
        public void ModExists_BuildCacheDirectoryNamedNameExists_ReturnsTrue() {
            var modDirectory = Path.Join(_workingDirectory, _mod.Name);
            _fileSystemMock.Directory.CreateDirectory(modDirectory); 
            var modsCache = InitializeModsCache();
            
            modsCache.ModExists(_mod).Should().BeTrue();
        }

        [Fact]
        public void ModExists_LoadCacheModCachedNoDirectory_ReturnsFalse() {
            var cacheFilePath = $"{_settingsMock.Object.ModsDirectory}\\{_settingsMock.Object.ModsManagerCacheFileName}.json";
            ISet<IMod> mods = new HashSet<IMod>();
            mods.Add(_mod);
            _fileSystemMock.File.WriteAllText(cacheFilePath, JsonConvert.SerializeObject(mods));

            var modsCache = InitializeModsCache();

            modsCache.ModExists(_mod).Should().BeFalse();
        }

        [Fact]
        public void ModExists_LoadCacheModCachedDirectoryPresent_ReturnsTrue() {
            var cacheFilePath = $"{_settingsMock.Object.ModsDirectory}\\{_settingsMock.Object.ModsManagerCacheFileName}.json";
            ISet<IMod> mods = new HashSet<IMod>();
            mods.Add(_mod);
            _fileSystemMock.File.WriteAllText(cacheFilePath, JsonConvert.SerializeObject(mods));
            _fileSystemMock.Directory.CreateDirectory(_mod.Directory);

            var modsCache = InitializeModsCache();

            modsCache.ModExists(_mod).Should().BeTrue();
        }

        private IMod FixtureCreateMod() {
            return _fixture.Create<Mod.Mod>();
        }

        private ModsCache InitializeModsCache() {
            return new ModsCache(_settingsMock.Object, _fileSystemMock);
        }

        public void Dispose() {
            if (_workingDirectory != null) _fileSystemMock.Directory.Delete(_workingDirectory, true);
        }
    }
}