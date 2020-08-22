using Arma.Server.Mod;
using Arma.Server.Test.Helpers;
using FluentAssertions;
using Xunit;

namespace Arma.Server.Test.Mod {
    public class ModTypeTests {
        [Fact]
        public void ModType_ApiServerSide_ParsedSuccessfully()
        {
            var _modSource = "server_side";
            var modSource = EnumConvert.ToEnum<ModType>(_modSource);
            modSource.Should().BeEquivalentTo(ModType.ServerSide);
        }

        [Fact]
        public void ModType_ApiRequired_ParsedSuccessfully()
        {
            var _modSource = "required";
            var modSource = EnumConvert.ToEnum<ModType>(_modSource);
            modSource.Should().BeEquivalentTo(ModType.Required);
        }

        [Fact]
        public void ModType_ApiOptional_ParsedSuccessfully()
        {
            var _modSource = "optional";
            var modSource = EnumConvert.ToEnum<ModType>(_modSource);
            modSource.Should().BeEquivalentTo(ModType.Optional);
        }

        [Fact]
        public void ModType_ApiClientSide_ParsedSuccessfully()
        {
            var _modSource = "client_side";
            var modSource = EnumConvert.ToEnum<ModType>(_modSource);
            modSource.Should().BeEquivalentTo(ModType.ClientSide);
        }
    }
}