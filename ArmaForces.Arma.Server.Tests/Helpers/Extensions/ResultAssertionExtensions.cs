using ArmaForces.Arma.Server.Common.Errors;
using CSharpFunctionalExtensions;
using FluentAssertions;
using FluentAssertions.Execution;

namespace ArmaForces.Arma.Server.Tests.Helpers.Extensions
{
    public static class ResultAssertionExtensions
    {
        public static void ShouldBeSuccess<TError>(this UnitResult<TError> result)
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

        public static void ShouldBeFailure<TError>(this UnitResult<TError> result, TError expectedError)
        {
            using (new AssertionScope())
            {
                result.IsFailure.Should().BeTrue();

                if (result.IsFailure)
                {
                    result.Error.Should().BeEquivalentTo(expectedError);
                }
            }
        }
        
        public static void ShouldBeFailure<TError>(this UnitResult<TError> result, string expectedErrorMessage)
            where TError: IError
        {
            using (new AssertionScope())
            {
                result.IsFailure.Should().BeTrue();

                if (result.IsFailure)
                {
                    result.Error.Message.Should().BeEquivalentTo(expectedErrorMessage);
                }
            }
        }

        public static void ShouldBeSuccess<T, E>(this Result<T, E> result)
        {
            result.IsSuccess.Should().BeTrue();
        }

        public static void ShouldBeSuccess<T, E>(this Result<T, E> result, T expectedValue)
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

        public static void ShouldBeFailure<T, E>(this Result<T, E> result, string expectedErrorMessage)
            where E : IError
        {
            using (new AssertionScope())
            {
                result.IsFailure.Should().BeTrue();

                if (result.IsFailure)
                {
                    result.Error.Message.Should().BeEquivalentTo(expectedErrorMessage);
                }
            }
        }

        public static void ShouldBeFailure<T, E>(this Result<T, E> result, E expectedError)
        {
            using (new AssertionScope())
            {
                result.IsFailure.Should().BeTrue();

                if (result.IsFailure)
                {
                    result.Error.Should().BeEquivalentTo(expectedError);
                }
            }
        }
    }
}
