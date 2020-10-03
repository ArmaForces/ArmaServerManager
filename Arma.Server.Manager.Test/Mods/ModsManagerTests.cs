using Arma.Server.Manager.Mods;
using Arma.Server.Mod;
using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arma.Server.Config;
using Arma.Server.Manager.Clients.Steam;
using Arma.Server.Modset;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Arma.Server.Manager.Test.Mods {
    public class ModsManagerTests {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IModsCache> _modsCacheMock = new Mock<IModsCache>();
        private readonly Mock<ISteamClient> _steamClientMock = new Mock<ISteamClient>();
        private readonly ModsManager _modsManager;
        private readonly IModset _modset;

        public ModsManagerTests() {
            _modset = new Modset.Modset {
                Mods = new HashSet<IMod> { FixtureCreateMod() }
            };

            _modsManager = new ModsManager(_steamClientMock.Object, _modsCacheMock.Object);
        }

        [Fact]
        public void PrepareModset_ModNotExists_DownloadsMod() {
            var modsEnumerable = _modset.Mods.Select(x => x.WorkshopId);

            _modsManager.PrepareModset(_modset);

            _steamClientMock.Verify(x => x.Download(modsEnumerable));
        }

        [Fact]
        public void PrepareModset_ModExists_DownloadsMod() {
            foreach (var mod in _modset.Mods) {
                _modsCacheMock.Setup(x => x.ModExists(It.IsAny<IMod>())).Returns(false);
                _modsCacheMock.Setup(x => x.ModExists(mod)).Returns(true);
            }

            _modsManager.PrepareModset(_modset);

            _steamClientMock.Verify(x => x.Download(It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        private IMod FixtureCreateMod() {
            return _fixture.Create<Mod.Mod>();
        }
    }
}