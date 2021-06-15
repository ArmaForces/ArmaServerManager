using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace ArmaForces.Arma.Server.Tests.Helpers.Extensions
{
    public static class ResultExtensions
    {
        public static AndConstraint<BooleanAssertions> ShouldBeSuccess(this Result result)
        {
            return result.IsSuccess.Should().BeTrue();
        }

        public static AndConstraint<BooleanAssertions> ShouldBeFailure(this Result result)
        {
            return result.IsFailure.Should().BeTrue();
        }
    }
}
