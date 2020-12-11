using System.Linq;
using Arma.Server.Config;
using Arma.Server.Extensions;
using Arma.Server.Mod;
using Arma.Server.Providers.Parameters;
using Arma.Server.Test.Helpers;
using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;

namespace Arma.Server.Test.Providers.Parameters
{
    public class HeadlessParametersProviderTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void GetStartupParams_AllParametersCorrect_AllParametersIncluded()
        {
            var hcProfileDirectory = _fixture.Create<string>();
            var serverCfgPath = _fixture.Create<string>();
            var basicCfgPath = _fixture.Create<string>();
            var port = _fixture.Create<int>();
            var serverPassword = _fixture.Create<string>();

            var modset = ModsetHelpers.CreateTestModset(_fixture);
            var requiredAndServerModsDirectories = modset.Mods
                .Where(x => x.Type == ModType.Required || x.Type == ModType.ServerSide)
                .GetDirectories();

            var modsetConfigMock = new Mock<IModsetConfig>();
            modsetConfigMock.Setup(x => x.HCProfileDirectory).Returns(hcProfileDirectory);
            modsetConfigMock.Setup(x => x.ServerCfg).Returns(serverCfgPath);
            modsetConfigMock.Setup(x => x.BasicCfg).Returns(basicCfgPath);
            modsetConfigMock.Setup(x => x.ServerPassword).Returns(serverPassword);

            var parametersProvider = new HeadlessParametersProvider(
                port,
                modset,
                modsetConfigMock.Object);

            var startupParams = parametersProvider.GetStartupParams();

            startupParams.Should()
                .Contain("-client")
                .And.Subject.Should()
                .Contain("-connect=127.0.0.1")
                .And.Subject.Should()
                .Contain($"port={port}")
                .And.Subject.Should()
                .Contain($"-password={serverPassword}")
                .And.Subject.Should()
                .Contain($"-profiles=\"{hcProfileDirectory}\"")
                .And.Subject.Should()
                .Contain($"-mod={string.Join(";", requiredAndServerModsDirectories)}");
        }

        [Fact]
        public void GetModsStartupParam_EmptyModset_ReturnsStringEmpty()
        {
            var modset = ModsetHelpers.CreateEmptyModset(_fixture);

            var modsParams = HeadlessParametersProvider.GetModsStartupParam(modset);

            modsParams.Should().BeNullOrWhiteSpace();
        }

        [Fact]
        public void GetModsStartupParam_ModsetWithRequiredMods_ReturnsStringWithModParam()
        {
            var modset = ModsetHelpers.CreateTestModsetWithModsOfOneType(_fixture, ModType.Required);

            var modsDirectories = modset.RequiredMods
                .GetDirectories();

            var modsParams = HeadlessParametersProvider.GetModsStartupParam(modset);

            modsParams.Should()
                .Contain($"-mod={string.Join(";", modsDirectories)}")
                .And.Subject.Should()
                .NotContain("-serverMod=");
        }

        [Fact]
        public void GetModsStartupParam_ModsetWithServerMods_ReturnsStringWithServerModParam()
        {
            var modset = ModsetHelpers.CreateTestModsetWithModsOfOneType(_fixture, ModType.ServerSide);

            var modsDirectories = modset.ServerSideMods
                .GetDirectories();

            var modsParams = HeadlessParametersProvider.GetModsStartupParam(modset);

            modsParams.Should()
                .Contain($"-mod={string.Join(";", modsDirectories)}")
                .And.Subject.Should()
                .NotContain("-serverMod=");
        }
    }
}
