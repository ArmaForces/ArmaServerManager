using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Steam.Content;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using AutoFixture;
using CSharpFunctionalExtensions;
using Moq;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Mods {
    public class ModsManagerUnitTests {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IModsCache> _modsCacheMock = new Mock<IModsCache>();
        private readonly Mock<IContentVerifier> _contentVerifierMock = new Mock<IContentVerifier>();
        private readonly Mock<IContentDownloader> _downloaderMock = new Mock<IContentDownloader>();
        private readonly ModsManager _modsManager;
        private readonly IModset _modset;

        public ModsManagerUnitTests() {
            _modset = new Modset {
                Mods = new HashSet<IMod> { FixtureCreateMod() }
            };

            _modsManager = new ModsManager(_downloaderMock.Object, _contentVerifierMock.Object, _modsCacheMock.Object);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task PrepareModset_ModNotExists_DownloadsMod() {
            _downloaderMock.Setup(x => x.DownloadOrUpdateMods(It.IsAny<IEnumerable<IMod>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<Result<IMod>> { Result.Success((IMod) new Mod()) }));

            _contentVerifierMock.Setup(x => x.ItemIsUpToDate(It.IsAny<ContentItem>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Result.Failure<ContentItem>("Item requires update")));

            await _modsManager.PrepareModset(_modset, CancellationToken.None);

            _downloaderMock.Verify(x => x.DownloadOrUpdateMods(
                It.IsAny<IEnumerable<IMod>>(),
                It.IsAny<CancellationToken>()));
        }

        [Fact, Trait("Category", "Unit")]
        public async Task PrepareModset_ModExists_DownloadsMod() {
            foreach (var mod in _modset.Mods) {
                _modsCacheMock.Setup(x => x.ModExists(It.IsAny<IMod>())).Returns(Task.FromResult(false));
                _modsCacheMock.Setup(x => x.ModExists(mod)).Returns(Task.FromResult(true));
            }

            _downloaderMock.Setup(x => x.DownloadOrUpdateMods(It.IsAny<IEnumerable<IMod>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<Result<IMod>> { Result.Success((IMod)new Mod()) }));

            _contentVerifierMock.Setup(x => x.ItemIsUpToDate(It.IsAny<ContentItem>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Result.Failure<ContentItem>("Item requires update")));

            await _modsManager.PrepareModset(_modset, CancellationToken.None);

            _downloaderMock.Verify(x => x.DownloadOrUpdateMods(
                It.IsAny<IEnumerable<IMod>>(),
                It.IsAny<CancellationToken>()));
        }

        private IMod FixtureCreateMod() {
            return _fixture.Create<Mod>();
        }
    }
}