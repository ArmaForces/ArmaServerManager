using System.Linq;
using Arma.Server.Config;
using Arma.Server.Mod;
using Arma.Server.Providers.Parameters;
using Arma.Server.Test.Helpers;
using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;

namespace Arma.Server.Test.Providers.Parameters
{
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

            var startupParams = parametersProvider.GetStartupParams();

            startupParams.Should()
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

        [Fact]
        public void GetModsStartupParam_EmptyModset_ReturnsStringEmpty()
        {
            var modset = ModsetHelpers.CreateEmptyModset(_fixture);

            var modsParams = ServerParametersProvider.GetModsStartupParam(modset);

            modsParams.Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public void GetModsStartupParam_ModsetWithRequiredMods_ReturnsStringWithModParam()
        {
            var modset = ModsetHelpers.CreateTestModsetWithModsOfOneType(_fixture, ModType.Required);

            var requiredModsDirectories = modset.RequiredMods
                .Select(x => x.Directory);

            var modsParams = ServerParametersProvider.GetModsStartupParam(modset);

            modsParams.Should()
                .Contain($"-mod={string.Join(";", requiredModsDirectories)}")
                .And.Subject.Should()
                .NotContain("-serverMod=");
        }

        [Fact]
        public void GetModsStartupParam_ModsetWithServerMods_ReturnsStringWithServerModParam()
        {
            var modset = ModsetHelpers.CreateTestModsetWithModsOfOneType(_fixture, ModType.ServerSide);

            var serverModsDirectories = modset.ServerSideMods
                .Select(x => x.Directory);

            var modsParams = ServerParametersProvider.GetModsStartupParam(modset);

            modsParams.Should()
                .Contain($"-serverMod={string.Join(";", serverModsDirectories)}")
                .And.Subject.Should()
                .NotContain("-mod=");
        }
    }
}
