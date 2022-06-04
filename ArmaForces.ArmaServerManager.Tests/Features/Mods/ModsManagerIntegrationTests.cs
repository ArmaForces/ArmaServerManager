using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Tests.Helpers;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Steam.Content;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Mods
{
    public class ModsManagerIntegrationTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly IServiceProvider _serviceProvider;

        public ModsManagerIntegrationTests()
        {
            var workingDirectory = Path.Join(Directory.GetCurrentDirectory(), "mods");
            var fileSystemMock = PrepareWorkingDirectory(workingDirectory);
            _serviceProvider = CreateServiceProvider(CreateSettingsMock(workingDirectory), fileSystemMock);
        }

        [Fact(Skip = "Uses actual connection to Steam"), Trait("Category", "Integration")]
        public void UpdateAllMods_CancellationRequested_TaskCancelled()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            
            var modsManager = _serviceProvider.GetService<IModsManager>()!;

            var task = modsManager.UpdateAllMods(cancellationTokenSource.Token);
            Func<Task> action = async () => await task;

            using (new AssertionScope())
            {
                action.Should().ThrowAsync<OperationCanceledException>();
                task.IsCanceled.Should().BeTrue();
            }
        }

        private static IFileSystem PrepareWorkingDirectory(string workingDirectory)
        {
            var fileSystemMock = new MockFileSystem();
            fileSystemMock.Directory.CreateDirectory(workingDirectory);

            return fileSystemMock;
        }

        private IServiceProvider CreateServiceProvider(
            ISettings settings,
            IFileSystem fileSystem)
        {
            return new ServiceCollection()
                .AddLogging()
                .AddArmaServer()
                .AddOrReplaceSingleton(settings)
                .AddSingleton(fileSystem)
                .AddTransient(_ => PrepareMockedModsCache())
                .AddTransient<IContentDownloader, ContentDownloader>()
                .AddTransient(_ => Mock.Of<IContentVerifier>())
                .AddTransient<IModsManager, ModsManager>()
                .BuildServiceProvider();
        }
        
        private IModsCache PrepareMockedModsCache()
        {
            var mock = new Mock<ModsCache>();

            var mods = ModHelpers.CreateModsList(_fixture);

            mock
                .Setup(x => x.Mods)
                .Returns(mods);

            return mock.Object;
        }

        private static ISettings CreateSettingsMock(string workingDirectory)
        {
            var settingsMock = new Mock<ISettings>();
            
            settingsMock
                .Setup(x => x.SteamUser)
                .Returns(string.Empty);
            settingsMock
                .Setup(x => x.SteamPassword)
                .Returns(string.Empty);
            settingsMock
                .Setup(x => x.ModsDirectory)
                .Returns(workingDirectory);

            return settingsMock.Object;
        }
    }
}
