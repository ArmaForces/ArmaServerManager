using Arma.Server.Mod;
using FluentAssertions;
using Xunit;

namespace Arma.Server.Test.Mod {
    public class ModSourceTests {
        [Fact]
        public void ModSource_ApiSteamWorkshop_ParsedSuccessfully()
        {
            var _modSource = "steam_workshop";
            var modSource = EnumConvert.ToEnum<ModSource>(_modSource);
            modSource.Should().Be(ModSource.SteamWorkshop);
        }

        [Fact]
        public void ModSource_ApiDirectory_ParsedSuccessfully()
        {
            var _modSource = "directory";
            var modSource = EnumConvert.ToEnum<ModSource>(_modSource);
            modSource.Should().Be(ModSource.Directory);
        }
    }
}