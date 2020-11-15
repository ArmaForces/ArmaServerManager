using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Clients.Steam;
using Arma.Server.Manager.Features.Mods;
using Arma.Server.Test.Helpers;
using AutoFixture;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace Arma.Server.Manager.Test.Features.Mods
{
    public class ModsDownloaderTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public async Task Download_4ModsAllSuccess()
        {
            const int modsNumber = 4;
            var modsToDownload = ModHelpers.CreateModsList(_fixture, modsNumber);

            var contentDownloaderMock = new Mock<IContentDownloader>();
            var expectedDownloadResults = new List<Result>
            {
                Result.Success(),
                Result.Success(),
                Result.Success(),
                Result.Success()
            };
            contentDownloaderMock.Setup(
                    x => x.DownloadOrUpdate(It.IsAny<List<KeyValuePair<int, ItemType>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(expectedDownloadResults));

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var modsDownloader = new ModsDownloader(contentDownloaderMock.Object);

            var downloadResults = await modsDownloader.DownloadOrUpdate(modsToDownload.Select(x => x.WorkshopId), cancellationToken);

            using (new AssertionScope())
            {
                downloadResults.Should().BeEquivalentTo(expectedDownloadResults);
                var keyValuePairs = modsToDownload.Select(x => new KeyValuePair<int, ItemType>(x.WorkshopId, ItemType.Mod));
                contentDownloaderMock.Verify(x => x.DownloadOrUpdate(keyValuePairs, cancellationToken));
            }
        }
    }
}
