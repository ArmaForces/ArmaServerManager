using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;

namespace ArmaForces.Arma.Server.Tests.Helpers.Extensions
{
    public static class ResultAssertionExtensions
    {
        public static void ShouldBeSuccess(this Result result)
        {
            using (new AssertionScope())
            {
                result.IsSuccess.Should().BeTrue();

                if (result.IsFailure)
                {
                    result.Error.Should().BeNull();
                }
            }
        }

        public static void ShouldBeSuccess<T>(this Result<T> result)
        {
            result.IsSuccess.Should().BeTrue();
        }

        public static void ShouldBeSuccess<T>(this Result<T> result, T expectedValue)
        {
            using (new AssertionScope())
            {
                result.IsSuccess.Should().BeTrue();

                if (result.IsSuccess)
                {
                    result.Value.Should().BeEquivalentTo(expectedValue);
                }
                else
                {
                    result.Error.Should().BeNull();
                }
            }
        }

        public static void ShouldBeFailure(this Result result, string expectedErrorMessage)
        {
            using (new AssertionScope())
            {
                result.IsFailure.Should().BeTrue();

                if (result.IsFailure)
                {
                    result.Error.Should().BeEquivalentTo(expectedErrorMessage);
                }
            }
        }
    }
}
