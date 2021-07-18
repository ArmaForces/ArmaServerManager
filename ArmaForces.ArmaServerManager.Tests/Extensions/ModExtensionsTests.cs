using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Extensions
{
    [Trait("Category", "Unit")]
    public class ModExtensionsTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void AsContentItem_ModWithAllData_ContentItemMatches()
        {
            var mod = _fixture.Create<Mod>();

            var contentItem = mod.AsContentItem();

            using (new AssertionScope())
            {
                contentItem.ItemType.Should().BeEquivalentTo(ItemType.Mod);
                contentItem.Id.Should().Be((uint) mod.WorkshopId);
                contentItem.Directory.Should().BeEquivalentTo(mod.Directory);
            }
        }

        [Fact]
        public void AsContentItem_ModWithMissingDirectory_ContentItemMatches()
        {
            var mod = _fixture.Create<Mod>();
            mod.Directory = null;

            var contentItem = mod.AsContentItem();

            using (new AssertionScope())
            {
                contentItem.ItemType.Should().BeEquivalentTo(ItemType.Mod);
                contentItem.Id.Should().Be((uint) mod.WorkshopId);
                contentItem.Directory.Should().BeNullOrWhiteSpace();
            }
        }
    }
}
