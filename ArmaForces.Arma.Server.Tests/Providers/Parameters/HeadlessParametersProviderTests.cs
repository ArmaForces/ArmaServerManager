using System.Linq;
using ArmaForces.Arma.Server.Config;
using ArmaForces.Arma.Server.Extensions;
using ArmaForces.Arma.Server.Features.Parameters.Providers;
using ArmaForces.Arma.Server.Tests.Helpers;
using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Providers.Parameters
{
    [Trait("Category", "Unit")]
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
            var requiredAndServerModsDirectories = modset.ServerSideMods
                .Concat(modset.RequiredMods)
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

            var startupParams = parametersProvider.GetStartupParams(string.Empty);

            startupParams.GetProcessStartInfo().Arguments.Should()
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
                .Contain($"-mod={string.Join(";", requiredAndServerModsDirectories)}")
                .And.Subject.Should()
                .NotContain("-serverMod=");
        }
    }
}
