using ArmaForces.ArmaServerManager.Features.Modsets.DTOs;
using ArmaForces.ArmaServerManager.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Features.Modsets.DTOs {
    public class WebModTypeTests {
        [Fact]
        public void ModType_ApiServerSide_ParsedSuccessfully()
        {
            var _modSource = "server_side";
            var modSource = EnumConvert.ToEnum<WebModType>(_modSource);
            modSource.Should().Be(WebModType.ServerSide);
        }

        [Fact]
        public void ModType_ApiRequired_ParsedSuccessfully()
        {
            var _modSource = "required";
            var modSource = EnumConvert.ToEnum<WebModType>(_modSource);
            modSource.Should().Be(WebModType.Required);
        }

        [Fact]
        public void ModType_ApiOptional_ParsedSuccessfully()
        {
            var _modSource = "optional";
            var modSource = EnumConvert.ToEnum<WebModType>(_modSource);
            modSource.Should().Be(WebModType.Optional);
        }

        [Fact]
        public void ModType_ApiClientSide_ParsedSuccessfully()
        {
            var _modSource = "client_side";
            var modSource = EnumConvert.ToEnum<WebModType>(_modSource);
            modSource.Should().Be(WebModType.ClientSide);
        }
    }
}