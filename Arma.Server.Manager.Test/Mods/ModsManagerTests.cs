using Arma.Server.Manager.Mods;
using Arma.Server.Mod;
using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Config;
using Arma.Server.Manager.Clients.Steam;
using Arma.Server.Modset;
using FluentAssertions.Execution;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Arma.Server.Manager.Test.Mods {
    public class ModsManagerTests {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IModsCache> _modsCacheMock = new Mock<IModsCache>();
        private readonly Mock<IModsDownloader> _downloaderMock = new Mock<IModsDownloader>();
        private readonly ModsManager _modsManager;
        private readonly IModset _modset;

        public ModsManagerTests() {
            _modset = new Modset.Modset {
                Mods = new HashSet<IMod> { FixtureCreateMod() }
            };

            _modsManager = new ModsManager(_downloaderMock.Object, _modsCacheMock.Object);
        }

        [Fact]
        public void PrepareModset_ModNotExists_DownloadsMod() {
            var modsEnumerable = _modset.Mods.Select(x => x.WorkshopId);

            _modsManager.PrepareModset(_modset);

            _downloaderMock.Verify(x => x.DownloadMods(modsEnumerable, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void PrepareModset_ModExists_DownloadsMod() {
            foreach (var mod in _modset.Mods) {
                _modsCacheMock.Setup(x => x.ModExists(It.IsAny<IMod>())).Returns(false);
                _modsCacheMock.Setup(x => x.ModExists(mod)).Returns(true);
            }

            _modsManager.PrepareModset(_modset);

            _downloaderMock.Verify(x => x.DownloadMods(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public void UpdateAllMods_CancellationRequested_TaskCancelled()
        {
            var settingsMock = new Mock<ISettings>();
            var fileSystemMock = new MockFileSystem();
            var workingDirectory = Path.Join(Directory.GetCurrentDirectory(), "mods");
            fileSystemMock.Directory.CreateDirectory(workingDirectory);
            settingsMock.Setup(x => x.SteamUser).Returns("");
            settingsMock.Setup(x => x.SteamPassword).Returns("");
            settingsMock.Setup(x => x.ModsDirectory).Returns(workingDirectory);
            var modsCache = new ModsCache(settingsMock.Object, fileSystemMock);
            var downloader = new ModsDownloader(settingsMock.Object);
            var modsManager = new ModsManager(downloader, modsCache);
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

            var task = modsManager.UpdateAllMods(cancellationTokenSource.Token);
            Func<Task> action = async () => await task;

            using (new AssertionScope())
            {
                action.Should().Throw<OperationCanceledException>();
                task.IsCanceled.Should().BeTrue();
            }
        }

        private IMod FixtureCreateMod() {
            return _fixture.Create<Mod.Mod>();
        }
    }
}