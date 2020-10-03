using Arma.Server.Manager.Clients.Modsets.Entities;
using FluentAssertions;
using Xunit;

namespace Arma.Server.Manager.Test.Clients.Modsets.Entities {
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