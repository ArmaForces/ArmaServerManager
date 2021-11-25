using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.ArmaServerManager.Extensions;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using AutoFixture;
using FluentAssertions;
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
            var expectedContentItem = CreateExpectedContentItem(mod);

            var contentItem = mod.AsContentItem();

            contentItem.Should().BeEquivalentTo(expectedContentItem);
        }

        [Fact]
        public void AsContentItem_ModWithMissingDirectory_ContentItemMatches()
        {
            var mod = _fixture.Build<Mod>()
                .Without(x => x.Directory)
                .Create();
            var expectedContentItem = CreateExpectedContentItem(mod);

            var contentItem = mod.AsContentItem();

            contentItem.Should().BeEquivalentTo(expectedContentItem);
        }

        private static ContentItem CreateExpectedContentItem(Mod mod) => new ContentItem
        {
            Id = (uint) mod.WorkshopId!,
            ItemType = ItemType.Mod,
            Directory = mod.Directory
        };
    }
}
