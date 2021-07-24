using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Tests.Helpers;
using ArmaForces.Arma.Server.Tests.Helpers.Extensions;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Mods;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Mods
{
    [Trait("Category", "Integration")]
    public class ModsCacheTests: IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly string _workingDirectory = Path.Join(Directory.GetCurrentDirectory(), "mods");
        private readonly ISettings _settingsMock;
        private readonly IFileSystem _fileSystemMock = new MockFileSystem();
        private readonly IMod _mod;
        
        private readonly IServiceProvider _testServiceProvider;

        public ModsCacheTests() {
            _fileSystemMock.Directory.CreateDirectory(_workingDirectory);
            _settingsMock = CreateSettings();

            _testServiceProvider = new ServiceCollection()
                .AddLogging()
                .AddArmaServer()
                .AddTransient<IModsCache, ModsCache>()
                .AddOrReplaceSingleton(_settingsMock)
                .AddSingleton(_fileSystemMock)
                .BuildServiceProvider();

            _mod = ModHelpers.CreateTestMod(_fixture);
        }

        [Fact]
        public async Task ModExists_DirectoryNotExists_ReturnsFalse() {
            var modsCache = GetModsCache();
            
            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeFalse();
                modsCache.Mods.Should().NotContain(_mod);
            }
        }

        [Fact]
        public async Task ModExists_DirectoryNamedIdExists_ReturnsTrue() {
            var modsCache = GetModsCache();
            var modDirectory = Path.Join(_workingDirectory, _mod.WorkshopId.ToString());
            _fileSystemMock.Directory.CreateDirectory(modDirectory);

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeTrue();
                modsCache.Mods.Should().ContainEquivalentOf(_mod);
            }
        }

        [Fact]
        public async Task ModExists_DirectoryNamedNameExists_ReturnsTrue() {
            var modsCache = GetModsCache();
            var modDirectory = Path.Join(_workingDirectory, _mod.Name);
            _fileSystemMock.Directory.CreateDirectory(modDirectory);

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeTrue();
                modsCache.Mods.Should().ContainEquivalentOf(_mod);
            }
        }

        [Fact]
        public async Task ModExists_DirectoryNamedWithAtExists_ReturnsTrue() {
            var modsCache = GetModsCache();
            var modDirectory = Path.Join(_workingDirectory, string.Join("", "@", _mod.Name));
            _fileSystemMock.Directory.CreateDirectory(modDirectory);

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeTrue();
                modsCache.Mods.Should().ContainEquivalentOf(_mod);
            }
        }

        [Fact]
        public async Task ModExists_CacheInvalidDirectoryRenamed_ReturnsTrue() {
            var oldModDirectory = Path.Join(_workingDirectory, _mod.WorkshopId.ToString());
            var newModDirectory = Path.Join(_workingDirectory, _mod.Name);
            _fileSystemMock.Directory.CreateDirectory(oldModDirectory);

            var modsCache = GetModsCache();
            _fileSystemMock.Directory.Delete(oldModDirectory);
            _fileSystemMock.Directory.CreateDirectory(newModDirectory);

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeTrue();
                modsCache.Mods.Should().Contain(mod => mod.WorkshopId == _mod.WorkshopId);
            }
        }

        [Fact]
        public async Task ModExists_BuildCacheDirectoryNamedNameExists_ReturnsTrue() {
            var modDirectory = Path.Join(_workingDirectory, _mod.Name);
            _fileSystemMock.Directory.CreateDirectory(modDirectory); 
            var modsCache = GetModsCache();

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeTrue();
                modsCache.Mods.Should().ContainEquivalentOf(_mod);
            }
        }

        [Fact]
        public async Task ModExists_LoadCacheModCachedNoDirectory_ReturnsFalse() {
            var cacheFilePath = $"{_settingsMock.ModsDirectory}\\{_settingsMock.ModsManagerCacheFileName}.json";
            ISet<IMod> mods = new HashSet<IMod>();
            mods.Add(_mod);
            await _fileSystemMock.File.WriteAllTextAsync(cacheFilePath, JsonConvert.SerializeObject(mods));

            var modsCache = GetModsCache();

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeFalse();
                modsCache.Mods.Should().NotContain(_mod);
            }
        }
        
        [Fact]
        public async Task ModExists_LoadCacheModCachedDirectoryPresent_ReturnsTrue() {
            var cacheFilePath = $"{_settingsMock.ModsDirectory}\\{_settingsMock.ModsManagerCacheFileName}.json";
            ISet<IMod> mods = new HashSet<IMod>();
            mods.Add(_mod);
            await _fileSystemMock.File.WriteAllTextAsync(cacheFilePath, JsonConvert.SerializeObject(mods));
            _fileSystemMock.Directory.CreateDirectory(_mod.Directory);

            var modsCache = GetModsCache();

            using (new AssertionScope())
            {
                (await modsCache.ModExists(_mod)).Should().BeTrue();
                modsCache.Mods.Should().ContainEquivalentOf(_mod);
            }
        }

        [Fact]
        public async Task AddOrUpdateModsInCache_SomeModsDataChanged_CacheUpdated()
        {
            var mods = ModHelpers.CreateModsList(_fixture);
            
            foreach (var mod in mods)
            {
                _fileSystemMock.Directory.CreateDirectory(mod.Directory);
                mod.Directory = _fixture.Create<string>();
            }

            var newMods = ModHelpers.CreateModsList(_fixture);
            var allMods = mods.Concat(newMods).ToList();

            var modsCache = GetModsCache();

            var result = await modsCache.AddOrUpdateModsInCache(allMods);

            result.ShouldBeSuccess(allMods.Cast<IMod>().ToList());
        }

        private ISettings CreateSettings()
        {
            var settingsMock = new Mock<ISettings>();
            
            settingsMock.Setup(x => x.ModsDirectory).Returns(_workingDirectory);
            settingsMock.Setup(x => x.ModsManagerCacheFileName).Returns(".ManagerModsCache");

            return settingsMock.Object;
        }
        
        private IModsCache GetModsCache() {
            return _testServiceProvider.GetService<IModsCache>()!;
        }

        public void Dispose() {
            _fileSystemMock.Directory.Delete(_workingDirectory, true);
        }
    }
}