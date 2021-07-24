using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using ArmaForces.ArmaServerManager.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Modsets.DTOs
{
    [Trait("Category", "Unit")]
    public class WebModSourceTests
    {
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