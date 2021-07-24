using System.Linq;
using ArmaForces.Arma.Server.Features.Mods;
using ArmaForces.Arma.Server.Features.Modsets;
using ArmaForces.Arma.Server.Tests.Helpers;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Features.Modsets
{
    [Trait("Category", "Unit")]
    public class ModsetTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void Equals_EqualEmptyModsets_ReturnsTrue()
        {
            var firstModset = ModsetHelpers.CreateEmptyModset(_fixture);
            var secondModset = ModsetHelpers.CopyModset(firstModset);

            var result = firstModset.Equals(secondModset);
            
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiredMods_ModsAndDlcs_Matches()
        {
            var modsList = ModHelpers.CreateModsList(_fixture);
            var dlcsList = DlcHelpers.CreateDlcsList(_fixture);

            var expectedCombinedList = modsList
                .Where(x => x.Type == ModType.Required)
                .Concat(dlcsList)
                .ToList();

            var modset = new Modset
            {
                Mods = modsList.Cast<IMod>().ToHashSet(),
                Dlcs = dlcsList.ToHashSet()
            };

            modset.RequiredMods.Should().BeEquivalentTo(expectedCombinedList);
        }
    }
}
