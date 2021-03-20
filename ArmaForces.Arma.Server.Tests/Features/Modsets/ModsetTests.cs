using ArmaForces.Arma.Server.Tests.Helpers;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Features.Modsets
{
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
    }
}
