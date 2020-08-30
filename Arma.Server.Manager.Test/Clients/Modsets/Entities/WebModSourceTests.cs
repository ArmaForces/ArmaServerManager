using Arma.Server.Manager.Clients.Modsets.Entities;
using FluentAssertions;
using Xunit;

namespace Arma.Server.Manager.Test.Clients.Modsets.Entities {
    public class WebModSourceTests {
        [Fact]
        public void ModSource_ApiSteamWorkshop_ParsedSuccessfully()
        {
            var _modSource = "steam_workshop";
            var modSource = EnumConvert.ToEnum<WebModSource>(_modSource);
            modSource.Should().Be(WebModSource.SteamWorkshop);
        }

        [Fact]
        public void ModSource_ApiDirectory_ParsedSuccessfully()
        {
            var _modSource = "directory";
            var modSource = EnumConvert.ToEnum<WebModSource>(_modSource);
            modSource.Should().Be(WebModSource.Directory);
        }
    }
}