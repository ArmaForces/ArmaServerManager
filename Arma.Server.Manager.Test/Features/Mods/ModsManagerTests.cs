using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Config;
using Arma.Server.Manager.Features.Mods;
using Arma.Server.Manager.Features.Steam.Content;
using Arma.Server.Manager.Features.Steam.Content.DTOs;
using Arma.Server.Mod;
using Arma.Server.Modset;
using AutoFixture;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace Arma.Server.Manager.Test.Features.Mods {
    public class ModsManagerTests {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IModsCache> _modsCacheMock = new Mock<IModsCache>();
        private readonly Mock<IContentVerifier> _contentVerifierMock = new Mock<IContentVerifier>();
        private readonly Mock<IContentDownloader> _downloaderMock = new Mock<IContentDownloader>();
        private readonly ModsManager _modsManager;
        private readonly IModset _modset;

        public ModsManagerTests() {
            _modset = new Modset.Modset {
                Mods = new HashSet<IMod> { FixtureCreateMod() }
            };

            _modsManager = new ModsManager(_downloaderMock.Object, _contentVerifierMock.Object, _modsCacheMock.Object);
        }

        [Fact]
        public async Task PrepareModset_ModNotExists_DownloadsMod() {
            _downloaderMock.Setup(x => x.DownloadOrUpdateMods(It.IsAny<IEnumerable<IMod>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<Result<IMod>> { Result.Success((IMod) new Mod.Mod()) }));

            await _modsManager.PrepareModset(_modset, CancellationToken.None);

            _downloaderMock.Verify(x => x.DownloadOrUpdateMods(
                It.IsAny<IEnumerable<IMod>>(),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task PrepareModset_ModExists_DownloadsMod() {
            foreach (var mod in _modset.Mods) {
                _modsCacheMock.Setup(x => x.ModExists(It.IsAny<IMod>())).Returns(Task.FromResult(false));
                _modsCacheMock.Setup(x => x.ModExists(mod)).Returns(Task.FromResult(true));
            }

            _downloaderMock.Setup(x => x.DownloadOrUpdateMods(It.IsAny<IEnumerable<IMod>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<Result<IMod>> { Result.Success((IMod)new Mod.Mod()) }));

            await _modsManager.PrepareModset(_modset, CancellationToken.None);

            _downloaderMock.Verify(x => x.DownloadOrUpdateMods(
                It.IsAny<IEnumerable<IMod>>(),
                It.IsAny<CancellationToken>()));
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
            var contentVerifier = new ContentVerifier(settingsMock.Object);
            var modsManager = new ModsManager(contentDownloader, contentVerifier, modsCache);
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