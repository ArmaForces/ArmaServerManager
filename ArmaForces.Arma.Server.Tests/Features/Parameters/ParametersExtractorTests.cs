using System;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Parameters;
using ArmaForces.Arma.Server.Tests.Helpers.Extensions;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Features.Parameters
{
    [Trait("Category", "Unit")]
    public class ParametersExtractorTests
    {
        private const string Parameters = @"""P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\arma3server_x64.exe"" -port=2302 ""-config=P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\serverConfig\modsetConfig\_Test\server.cfg"" ""-cfg=P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\serverConfig\modsetConfig\_Test\basic.cfg"" -profiles=""P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\serverConfig\modsetConfig\_Test\profiles\Server"" -modsetName=_Test -startTime=2021-11-25T22:04:00+01:00 -name=server -filePatching -netlog -limitFPS=100 -loadMissionToMemory";

        [Fact]
        public async Task ExtractParameters()
        {
            var parametersExtractor = new ParametersExtractor();
            var expectedParameters = new ProcessParameters(
                processPath: @"P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\arma3server_x64.exe",
                client: false,
                port: 2302,
                serverCfgPath: @"P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\serverConfig\modsetConfig\_Test\server.cfg",
                basicCfgPath: @"P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\serverConfig\modsetConfig\_Test\basic.cfg",
                profilePath: @"P:\Program Files (x86)\SteamLibrary\steamapps\common\Arma 3\serverConfig\modsetConfig\_Test\profiles\Server",
                name: "server",
                modsetName: "_Test",
                startTime: new DateTimeOffset(2021, 11, 25, 22, 04, 00, offset: TimeSpan.FromHours(1)),
                filePatching: true,
                netLog: true,
                fpsLimit: 100,
                loadMissionToMemory: true);

            var extractParametersResult = await parametersExtractor.ExtractParameters(Parameters);

            extractParametersResult.ShouldBeSuccess(expectedParameters);
        }
    }
}
