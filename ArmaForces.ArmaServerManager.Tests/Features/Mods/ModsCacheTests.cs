using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Mods;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Mods {
    public class ModsCacheTests: IDisposable {
        private readonly Fixture _fixture = new Fixture();
        private readonly string _workingDirectory = Path.Join(Directory.GetCurrentDirectory(), "mods");
        private readonly Mock<ISettings> _settingsMock = new Mock<ISettings>();
        private readonly IModDirectoryFinder _modDirectoryFinder;
        private readonly IFileSystem _fileSystemMock = new MockFileSystem();
        private readonly IMod _mod;

        public ModsCacheTests() {
            _fileSystemMock.Directory.CreateDirectory(_workingDirectory);
            _settingsMock.Setup(x => x.ModsDirectory).Returns(_workingDirectory);
            _settingsMock.Setup(x => x.ModsManagerCacheFileName).Returns(".ManagerModsCache");

            _modDirectoryFinder = new ModDirectoryFinder(
                _settingsMock.Object,
                new NullLogger<ModDirectoryFinder>(),
                _fileSystemMock);

            _mod = FixtureCreateMod();
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ModExists_DirectoryNotExists_ReturnsFalse() {
            var modsCache = InitializeModsCache();
            
            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeFalse();
                modsCache.Mods.Should().NotContain(_mod);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ModExists_DirectoryNamedIdExists_ReturnsTrue() {
            var modsCache = InitializeModsCache();
            var modDirectory = Path.Join(_workingDirectory, _mod.WorkshopId.ToString());
            _fileSystemMock.Directory.CreateDirectory(modDirectory);

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeTrue();
                modsCache.Mods.Should().ContainEquivalentOf(_mod);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ModExists_DirectoryNamedNameExists_ReturnsTrue() {
            var modsCache = InitializeModsCache();
            var modDirectory = Path.Join(_workingDirectory, _mod.Name);
            _fileSystemMock.Directory.CreateDirectory(modDirectory);

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeTrue();
                modsCache.Mods.Should().ContainEquivalentOf(_mod);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ModExists_DirectoryNamedWithAtExists_ReturnsTrue() {
            var modsCache = InitializeModsCache();
            var modDirectory = Path.Join(_workingDirectory, string.Join("", "@", _mod.Name));
            _fileSystemMock.Directory.CreateDirectory(modDirectory);

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeTrue();
                modsCache.Mods.Should().ContainEquivalentOf(_mod);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ModExists_CacheInvalidDirectoryRenamed_ReturnsTrue() {
            var oldModDirectory = Path.Join(_workingDirectory, _mod.WorkshopId.ToString());
            var newModDirectory = Path.Join(_workingDirectory, _mod.Name);
            _fileSystemMock.Directory.CreateDirectory(oldModDirectory);

            var modsCache = InitializeModsCache();
            _fileSystemMock.Directory.Delete(oldModDirectory);
            _fileSystemMock.Directory.CreateDirectory(newModDirectory);

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeTrue();
                modsCache.Mods.Should().Contain(mod => mod.WorkshopId == _mod.WorkshopId);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ModExists_BuildCacheDirectoryNamedNameExists_ReturnsTrue() {
            var modDirectory = Path.Join(_workingDirectory, _mod.Name);
            _fileSystemMock.Directory.CreateDirectory(modDirectory); 
            var modsCache = InitializeModsCache();

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeTrue();
                modsCache.Mods.Should().ContainEquivalentOf(_mod);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ModExists_LoadCacheModCachedNoDirectory_ReturnsFalse() {
            var cacheFilePath = $"{_settingsMock.Object.ModsDirectory}\\{_settingsMock.Object.ModsManagerCacheFileName}.json";
            ISet<IMod> mods = new HashSet<IMod>();
            mods.Add(_mod);
            await _fileSystemMock.File.WriteAllTextAsync(cacheFilePath, JsonConvert.SerializeObject(mods));

            var modsCache = InitializeModsCache();

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeFalse();
                modsCache.Mods.Should().NotContain(_mod);
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ModExists_LoadCacheModCachedDirectoryPresent_ReturnsTrue() {
            var cacheFilePath = $"{_settingsMock.Object.ModsDirectory}\\{_settingsMock.Object.ModsManagerCacheFileName}.json";
            ISet<IMod> mods = new HashSet<IMod>();
            mods.Add(_mod);
            await _fileSystemMock.File.WriteAllTextAsync(cacheFilePath, JsonConvert.SerializeObject(mods));
            _fileSystemMock.Directory.CreateDirectory(_mod.Directory);

            var modsCache = InitializeModsCache();

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeTrue();
                modsCache.Mods.Should().ContainEquivalentOf(_mod);
            }
        }

        private IMod FixtureCreateMod() {
            return _fixture.Create<Mod>();
        }

        private ModsCache InitializeModsCache() {
            return new ModsCache(_settingsMock.Object, _modDirectoryFinder, _fileSystemMock);
        }

        public void Dispose() {
            _fileSystemMock.Directory.Delete(_workingDirectory, true);
        }
    }
}