using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Features.Parameters.Providers;
using ArmaForces.Arma.Server.Tests.Helpers;
using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Providers.Parameters
{
    [Trait("Category", "Unit")]
    public class ServerParametersProviderTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void GetStartupParams_AllParametersCorrect_AllParametersIncluded()
        {
            var serverProfileDirectory = _fixture.Create<string>();
            var serverCfgPath = _fixture.Create<string>();
            var basicCfgPath = _fixture.Create<string>();
            var port = _fixture.Create<int>();

            var modset = ModsetHelpers.CreateTestModset(_fixture);
            var serverModsDirectories = modset.ServerSideMods
                .Select(x => x.Directory);
            var requiredModsDirectories = modset.RequiredMods
                .Select(x => x.Directory);

            var modsetConfigMock = new Mock<IModsetConfig>();
            modsetConfigMock.Setup(x => x.ServerProfileDirectory).Returns(serverProfileDirectory);
            modsetConfigMock.Setup(x => x.ServerCfg).Returns(serverCfgPath);
            modsetConfigMock.Setup(x => x.BasicCfg).Returns(basicCfgPath);

            var parametersProvider = new ServerParametersProvider(
                port,
                modset,
                modsetConfigMock.Object);

            var startupParams = parametersProvider.GetStartupParams(string.Empty);

            startupParams.GetProcessStartInfo().Arguments.Should()
                .Contain($"port={port}")
                .And.Subject.Should()
                .Contain($"-config={serverCfgPath}")
                .And.Subject.Should()
                .Contain($"-cfg={basicCfgPath}")
                .And.Subject.Should()
                .Contain($"-profiles=\"{serverProfileDirectory}\"")
                .And.Subject.Should()
                .Contain($"-mod={string.Join(";", requiredModsDirectories)}")
                .And.Subject.Should()
                .Contain($"-serverMod={string.Join(";", serverModsDirectories)}");
        }
    }
}
