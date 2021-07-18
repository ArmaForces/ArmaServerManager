using System;
using System.Collections;
using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Extensions;
using FluentAssertions;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Extensions
{
    [Trait("Category", "Unit")]
    public class DateTimeExtensionsTests
    {
        [Theory]
        [ClassData(typeof(DateTimeTestData))]
        public void IsCloseTo_ClassTestData(
            DateTime dateTime,
            DateTime referenceDateTime,
            TimeSpan precision,
            bool expectedResult)
        {
            var result = dateTime.IsCloseTo(referenceDateTime, precision);

            result.Should().Be(expectedResult);
        }
    }

    public class DateTimeTestData : IEnumerable<object[]>
    {
        private readonly TimeSpan _defaultPrecision = TimeSpan.FromMinutes(15);

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] {DateTime.Now, DateTime.Now - TimeSpan.FromMinutes(10), _defaultPrecision, true};
            yield return new object[] {DateTime.Now, DateTime.Now - TimeSpan.FromMinutes(20), _defaultPrecision, false};
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
