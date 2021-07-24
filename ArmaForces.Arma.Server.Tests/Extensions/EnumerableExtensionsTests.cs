using System.Collections.Generic;
using System.Linq;
using ArmaForces.Arma.Server.Extensions;
using FluentAssertions;
using Xunit;

namespace ArmaForces.Arma.Server.Tests.Extensions
{
    [Trait("Category", "Unit")]
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void AsList_SingleItem_ConvertedToOneItemList()
        {
            const string someObject = "test";

            var list = someObject.AsList();

            list.Should().ContainSingle(x => x == someObject);
        }

        [Fact]
        public void AsList_List_ConvertedToOneItemListOfLists()
        {
            var list = new List<string>();

            var newList = list.AsList();

            newList.Should().ContainSingle(x => x == list);
        }
        
        [Theory]
        [InlineData(new string[0], true)]
        [InlineData(new[] {"", ""}, false)]
        public void IsEmpty(IEnumerable<string> enumerable, bool expectedEmpty)
        {
            enumerable.IsEmpty().Should().Be(expectedEmpty);
        }

        [Theory]
        [InlineData(new[] {"Test", "Test2"}, "Test", false)]
        [InlineData(new[] {"Test", "Test2"}, "Test3", true)]
        [InlineData(new string[0], "Test", true)]
        public void NotContains(
            IEnumerable<string> enumerable,
            string value,
            bool shouldContain)
        {
            enumerable.NotContains(value).Should().Be(shouldContain);
        }

        [Fact]
        public void WhereNotNull_ListOfStringWithSomeNulls_FiltersOutNulls()
        {
            var list = new List<string?>
            {
                "Test",
                null,
                "Test2"
            };

            var expectedList = new List<string>
            {
                "Test",
                "Test2"
            };

            var whereNotNull = list.WhereNotNull().ToList();

            whereNotNull.Should().BeEquivalentTo(expectedList);
        }

        [Fact]
        public void WhereNotNull_ListOfIntsWithSomeNulls_FiltersOutNulls()
        {
            var list = new List<int?>
            {
                0,
                null,
                2
            };

            var expectedList = new List<int>
            {
                0,
                2
            };

            var whereNotNull = list.WhereNotNull().ToList();

            whereNotNull.Should().BeEquivalentTo(expectedList);
        }
    }
}
