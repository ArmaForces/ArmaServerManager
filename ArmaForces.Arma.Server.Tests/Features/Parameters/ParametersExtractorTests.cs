using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Parameters;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Features.Parameters
{
    [Trait("Category", "Unit")]
    public class ParametersExtractorTests
    {
        private const string Parameters = @"""P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\arma3server_x64.exe"" -port=2302 ""-config=P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\serverConfig\modsetConfig\_Test\server.cfg"" ""-cfg=P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\serverConfig\modsetConfig\_Test\basic.cfg"" -profiles=""P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\serverConfig\modsetConfig\_Test\profiles\Server"" -modsetName=_Test -name=server -filePatching -netlog -limitFPS=100 -loadMissionToMemory";

        [Fact]
        public async Task ExtractParameters()
        {
            var parametersExtractor = new ParametersExtractor();

            var extractParametersResult = await parametersExtractor.ExtractParameters(Parameters);

            using (new AssertionScope())
            {
                extractParametersResult.IsSuccess.Should().BeTrue();
                var parameters = extractParametersResult.Value;
                parameters.Port.Should().Be(2302);
                parameters.ModsetName.Should().Be("_Test");
                parameters.Name.Should().Be("server");

                parameters.ProcessPath
                    .Should().Be(@"P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\arma3server_x64.exe");
                parameters.ServerCfgPath
                    .Should().Be(@"P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\serverConfig\modsetConfig\_Test\server.cfg");
                parameters.BasicCfgPath
                    .Should().Be(@"P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\serverConfig\modsetConfig\_Test\basic.cfg");
                parameters.ProfilePath
                    .Should().Be(@"P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\serverConfig\modsetConfig\_Test\profiles\Server");

                parameters.Client.Should().BeFalse();
                parameters.LimitFPS.Should().Be(100);
                parameters.FilePatching.Should().BeTrue();
                parameters.NetLog.Should().BeTrue();
                parameters.LoadMissionToMemory.Should().BeTrue();
            }
        }
    }
}
