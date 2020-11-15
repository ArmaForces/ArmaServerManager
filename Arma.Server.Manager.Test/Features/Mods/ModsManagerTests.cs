using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Config;
using Arma.Server.Manager.Clients.Steam;
using Arma.Server.Manager.Features.Mods;
using Arma.Server.Mod;
using Arma.Server.Modset;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace Arma.Server.Manager.Test.Features.Mods {
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
        public async Task PrepareModset_ModNotExists_DownloadsMod() {
            var modsEnumerable = _modset.Mods.Select(x => x.WorkshopId);

            await _modsManager.PrepareModset(_modset);

            _downloaderMock.Verify(x => x.Download(modsEnumerable, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task PrepareModset_ModExists_DownloadsMod() {
            foreach (var mod in _modset.Mods) {
                _modsCacheMock.Setup(x => x.ModExists(It.IsAny<IMod>())).Returns(Task.FromResult(false));
                _modsCacheMock.Setup(x => x.ModExists(mod)).Returns(Task.FromResult(true));
            }

            await _modsManager.PrepareModset(_modset);

            _downloaderMock.Verify(x => x.Download(
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
            var contentDownloader = new ContentDownloader(settingsMock.Object);
            var modsDownloader = new ModsDownloader(contentDownloader);
            var modsManager = new ModsManager(modsDownloader, modsCache);
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