using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Tests.Helpers;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Steam.Content;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using AutoFixture;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Mods
{
    [Trait("Category", "Unit")]
    public class ModsManagerUnitTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IModsCache> _modsCacheMock;
        private readonly Mock<IContentVerifier> _contentVerifierMock;
        private readonly Mock<IContentDownloader> _downloaderMock;
        private readonly ModsManager _modsManager;

        public ModsManagerUnitTests()
        {
            _modsCacheMock = CreateModsCacheMock();
            _contentVerifierMock = CreateContentVerifierMock();
            _downloaderMock = CreateContentDownloaderMock();
            _modsManager = new ModsManager(
                _downloaderMock.Object,
                _contentVerifierMock.Object,
                _modsCacheMock.Object,
                new NullLogger<ModsManager>());
        }

        [Fact]
        public async Task PrepareModset_AllModsExistAndAreUpToDate_NothingHappens()
        {
            var modset = ModsetHelpers.CreateTestModset(_fixture);
            var cancellationToken = CancellationToken.None;
            
            AddModsToModsCache(modset.Mods.ToList());
            SetModsAsUpToDate(modset.Mods.ToList());

            await _modsManager.PrepareModset(modset, cancellationToken);
            
            VerifyModsNotDownloadedOrUpdated();
            VerifyModsCacheNotUpdated();
        }

        [Fact]
        public async Task PrepareModset_SomeModsDontExist_DownloadsMissingMods()
        {
            var modset = ModsetHelpers.CreateTestModset(_fixture);
            var cancellationToken = CancellationToken.None;

            var existingMods = modset.Mods
                .Take(5)
                .ToList();
            var missingMods = modset.Mods
                .Except(existingMods)
                .ToList();
            
            AddModsToModsCache(existingMods);
            SetModsAsUpToDate(existingMods);
            SetupContentDownloader(missingMods);

            await _modsManager.PrepareModset(modset, cancellationToken);

            VerifyModsDownloadedOrUpdated(missingMods, cancellationToken);
            VerifyModsCacheUpdated(missingMods);
        }

        [Fact]
        public async Task PrepareModset_SomeModsOutdated_UpdatedOutdatedMods()
        {
            var modset = ModsetHelpers.CreateTestModset(_fixture);
            var cancellationToken = CancellationToken.None;

            var outdatedMods = modset.Mods
                .Take(5)
                .ToList();
            var upToDateMods = modset.Mods
                .Except(outdatedMods)
                .ToList();
            
            AddModsToModsCache(modset.Mods.ToList());
            SetModsAsUpToDate(upToDateMods);
            SetupContentDownloader(outdatedMods);

            await _modsManager.PrepareModset(modset, cancellationToken);
            
            VerifyModsDownloadedOrUpdated(outdatedMods, cancellationToken);
            VerifyModsCacheUpdated(outdatedMods);
        }

        private void VerifyModsDownloadedOrUpdated(
            IReadOnlyCollection<IMod> missingMods,
            CancellationToken? cancellationToken = null)
            => _downloaderMock.Verify(
                x => x.DownloadOrUpdateMods(
                    It.Is<IReadOnlyCollection<IMod>>(mods => mods.All(missingMods.Contains)),
                    cancellationToken ?? CancellationToken.None));

        private void VerifyModsCacheUpdated(IReadOnlyCollection<IMod> modsShouldBeUpdated) => _modsCacheMock.Verify(
            x => x.AddOrUpdateModsInCache(
                It.Is<IReadOnlyCollection<IMod>>(mods => mods.All(modsShouldBeUpdated.Contains))),
            Times.Once);

        private void VerifyModsCacheNotUpdated() => _modsCacheMock.Verify(
            x => x.AddOrUpdateModsInCache(It.IsAny<IReadOnlyCollection<IMod>>()),
            Times.Never);

        private void VerifyModsNotDownloadedOrUpdated()
        {
            _downloaderMock.Verify(
                x => x.DownloadOrUpdateMods(
                    It.IsAny<IReadOnlyCollection<IMod>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private void SetupContentDownloader(
            IReadOnlyCollection<IMod>? modsToSuccessfullyDownload = null,
            IReadOnlyCollection<IMod>? modsToFailDownload = null)
        {
            modsToSuccessfullyDownload ??= new List<IMod>();
            modsToFailDownload ??= new List<IMod>();

            var modsToDownload = modsToSuccessfullyDownload
                .Concat(modsToFailDownload)
                .ToList();

            var successResults = modsToSuccessfullyDownload
                .Select(Result.Success);

            var failedResults = modsToFailDownload
                .Select(x => Result.Failure<IMod>($"Mod {x} failed to download"));

            _downloaderMock
                .Setup(x => x.DownloadOrUpdateMods(modsToDownload, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(successResults.Concat(failedResults).ToList()));
        }

        private void SetModsAsUpToDate(IReadOnlyCollection<IMod> mods)
        {
            foreach (var mod in mods)
            {
                var contentItem = mod.AsContentItem();
                
                _contentVerifierMock
                    .Setup(x => x.ItemIsUpToDate(
                        It.Is<ContentItem>(item => contentItem.Equals(item)),
                        It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(Result.Success(contentItem)));
            }
        }

        private void AddModsToModsCache(IReadOnlyCollection<IMod> mods)
        {
            foreach (var mod in mods)
            {
                _modsCacheMock
                    .Setup(x => x.ModExists(mod))
                    .Returns(Task.FromResult(true));
            }
        }

        private Mock<IModsCache> CreateModsCacheMock()
        {
            var mock = new Mock<IModsCache>();
            
            mock
                .Setup(x => x.ModExists(It.IsAny<IMod>()))
                .Returns(Task.FromResult(false));
            
            return mock;
        }

        private Mock<IContentVerifier> CreateContentVerifierMock()
        {
            var mock = new Mock<IContentVerifier>();

            mock
                .Setup(x => x.ItemIsUpToDate(It.IsAny<ContentItem>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Result.Failure<ContentItem>("Item requires update")));
            
            return mock;
        }

        private Mock<IContentDownloader> CreateContentDownloaderMock()
        {
            var mock = new Mock<IContentDownloader>();
            
            mock
                .Setup(x => x.DownloadOrUpdateMods(It.IsAny<IReadOnlyCollection<IMod>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<Result<IMod>>{Result.Failure<IMod>("Item could not be downloaded")}));

            return mock;
        }
    }
}