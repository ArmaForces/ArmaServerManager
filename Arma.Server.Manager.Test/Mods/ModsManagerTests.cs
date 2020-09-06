using Arma.Server.Manager.Mods;
using Arma.Server.Mod;
using AutoFixture;
using FluentAssertions;
using System;
using System.IO;
using Arma.Server.Config;
using Moq;
using Xunit;

namespace Arma.Server.Manager.Test.Mods {
    public class ModsCacheTests: IDisposable {
        private readonly Fixture _fixture = new Fixture();
        private readonly string _workingDirectory = Path.Join(Directory.GetCurrentDirectory(), "mods");
        private readonly Mock<ISettings> _settingsMock = new Mock<ISettings>();
        private readonly IMod _mod;

        public ModsCacheTests() {
            Directory.CreateDirectory(_workingDirectory);
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
            Directory.CreateDirectory(modDirectory);

            modsCache.ModExists(_mod).Should().BeTrue();
        }

        [Fact]
        public void ModExists_DirectoryNamedNameExists_ReturnsTrue() {
            var modsCache = InitializeModsCache();
            var modDirectory = Path.Join(_workingDirectory, _mod.Name);
            Directory.CreateDirectory(modDirectory);

            modsCache.ModExists(_mod).Should().BeTrue();
        }

        [Fact]
        public void ModExists_DirectoryNamedWithAtExists_ReturnsTrue() {
            var modsCache = InitializeModsCache();
            var modDirectory = Path.Join(_workingDirectory, string.Join("@", _mod.Name));
            Directory.CreateDirectory(modDirectory);

            modsCache.ModExists(_mod).Should().BeTrue();
        }

        [Fact]
        public void ModExists_CacheInvalidDirectoryRenamed_ReturnsTrue() {
            var oldModDirectory = Path.Join(_workingDirectory, _mod.WorkshopId.ToString());
            var newModDirectory = Path.Join(_workingDirectory, _mod.Name);
            Directory.CreateDirectory(oldModDirectory);

            var modsCache = InitializeModsCache();
            Directory.Delete(oldModDirectory);
            Directory.CreateDirectory(newModDirectory);

            modsCache.ModExists(_mod).Should().BeTrue();
        }

        private IMod FixtureCreateMod() {
            return _fixture.Create<Mod.Mod>();
        }

        private ModsCache InitializeModsCache() {
            return new ModsCache(_settingsMock.Object);
        }

        public void Dispose() {
            if (_workingDirectory != null) Directory.Delete(_workingDirectory, true);
        }
    }
}