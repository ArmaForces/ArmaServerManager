using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.ArmaServerManager.Features.Mods;
using ArmaForces.ArmaServerManager.Features.Steam.Content;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Mods
{
    public class ModsManagerIntegrationTests
    {
        private readonly IServiceProvider _serviceProvider;

        public ModsManagerIntegrationTests()
        {
            var workingDirectory = Path.Join(Directory.GetCurrentDirectory(), "mods");
            var fileSystemMock = PrepareWorkingDirectory(workingDirectory);
            _serviceProvider = CreateServiceProvider(CreateSettingsMock(workingDirectory), fileSystemMock);
        }

        [Fact, Trait("Category", "Integration")]
        public void UpdateAllMods_CancellationRequested_TaskCancelled()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            
            var modsManager = _serviceProvider.GetService<IModsManager>()!;

            var task = modsManager.UpdateAllMods(cancellationTokenSource.Token);
            Func<Task> action = async () => await task;

            using (new AssertionScope())
            {
                action.Should().Throw<OperationCanceledException>();
                task.IsCanceled.Should().BeTrue();
            }
        }

        private static IFileSystem PrepareWorkingDirectory(string workingDirectory)
        {
            var fileSystemMock = new MockFileSystem();
            fileSystemMock.Directory.CreateDirectory(workingDirectory);

            return fileSystemMock;
        }

        private static IServiceProvider CreateServiceProvider(
            ISettings settings,
            IFileSystem fileSystem)
        {
            return new ServiceCollection()
                .AddLogging()
                .AddArmaServer()
                .Replace(new ServiceDescriptor(typeof(ISettings), settings))
                .AddSingleton(fileSystem)
                .AddTransient<IModsCache, ModsCache>()
                .AddTransient<IContentDownloader, ContentDownloader>()
                .AddTransient(_ => Mock.Of<IContentVerifier>())
                .AddTransient<IModsManager, ModsManager>()
                .BuildServiceProvider();
        }

        private static ISettings CreateSettingsMock(string workingDirectory)
        {
            var settingsMock = new Mock<ISettings>();
            
            settingsMock.Setup(x => x.SteamUser).Returns("");
            settingsMock.Setup(x => x.SteamPassword).Returns("");
            settingsMock.Setup(x => x.ModsDirectory).Returns(workingDirectory);

            return settingsMock.Object;
        }
    }
}
