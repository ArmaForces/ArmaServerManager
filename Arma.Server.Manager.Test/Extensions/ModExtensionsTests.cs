using Arma.Server.Manager.Extensions;
using Arma.Server.Manager.Features.Steam.Content.DTOs;
using AutoFixture;
using BytexDigital.Steam.Core.Structs;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Arma.Server.Manager.Test.Extensions
{
    public class ModExtensionsTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void AsContentItem_ModWithAllData_ContentItemMatches()
        {
            var mod = _fixture.Create<Mod.Mod>();

            var contentItem = mod.AsContentItem();

            using (new AssertionScope())
            {
                contentItem.ItemType.Should().BeEquivalentTo(ItemType.Mod);
                contentItem.Id.Should().Be((uint) mod.WorkshopId);
                contentItem.Directory.Should().BeEquivalentTo(mod.Directory);
                contentItem.ManifestId.Should().Be(new ManifestId(mod.ManifestId!.Value));
            }
        }

        [Fact]
        public void AsContentItem_ModWithMissingDirectory_ContentItemMatches()
        {
            var mod = _fixture.Create<Mod.Mod>();
            mod.Directory = null;

            var contentItem = mod.AsContentItem();

            using (new AssertionScope())
            {
                contentItem.ItemType.Should().BeEquivalentTo(ItemType.Mod);
                contentItem.Id.Should().Be((uint) mod.WorkshopId);
                contentItem.Directory.Should().BeNullOrWhiteSpace();
                contentItem.ManifestId.Should().Be(new ManifestId(mod.ManifestId!.Value));
            }
        }

        [Fact]
        public void AsContentItem_ModWithMissingManifestId_ContentItemMatches()
        {
            var mod = _fixture.Create<Mod.Mod>();
            mod.ManifestId = null;

            var contentItem = mod.AsContentItem();

            using (new AssertionScope())
            {
                contentItem.ItemType.Should().BeEquivalentTo(ItemType.Mod);
                contentItem.Id.Should().Be((uint) mod.WorkshopId);
                contentItem.Directory.Should().Be(mod.Directory);
                contentItem.ManifestId.Should().BeNull();
            }
        }
    }
}
