using Arma.Server.Manager.Mods;
using Arma.Server.Mod;
using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arma.Server.Config;
using Arma.Server.Manager.Steam;
using Arma.Server.Modset;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Arma.Server.Manager.Test.Mods {
    public class ModsManagerTests: IDisposable {
        private readonly Fixture _fixture = new Fixture();
        private readonly string _workingDirectory = Path.Join(Directory.GetCurrentDirectory(), "mods");
        private readonly Mock<ISettings> _settingsMock = new Mock<ISettings>();
        private readonly IModset _modset;

        public ModsManagerTests() {
            Directory.CreateDirectory(_workingDirectory);
            _settingsMock.Setup(x => x.ModsDirectory).Returns(_workingDirectory);
            _settingsMock.Setup(x => x.ModsManagerCacheFileName).Returns(".ManagerModsCache");
            var mod = FixtureCreateMod();
            _modset = new Modset.Modset {
                Mods = new HashSet<IMod> { mod }
            };
        }

        [Fact]
        public void PrepareModset_ModNotExists_DownloadsMod() {
            var steamClientMock = new Mock<IClient>();
            var modsEnumerable = _modset.Mods.Select(x => x.WorkshopId);
            var modsManager = new ModsManager(_settingsMock.Object, steamClientMock.Object);

            modsManager.PrepareModset(_modset);

            steamClientMock.Verify(x => x.Download(modsEnumerable));
        }

        [Fact]
        public void PrepareModset_ModExists_DownloadsMod() {
            var steamClientMock = new Mock<IClient>();
            foreach (var modId in _modset.Mods.Select(x => x.WorkshopId)) {
                Directory.CreateDirectory(Path.Join(_workingDirectory, modId.ToString()));
            }
            var modsManager = new ModsManager(_settingsMock.Object, steamClientMock.Object);

            modsManager.PrepareModset(_modset);

            steamClientMock.Verify(x => x.Download(It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        private IMod FixtureCreateMod() {
            return _fixture.Create<Mod.Mod>();
        }

        public void Dispose() {
            if (_workingDirectory != null) Directory.Delete(_workingDirectory, true);
        }
    }
}