using System;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Extensions;
using AutoFixture;
using CSharpFunctionalExtensions;
using Moq;
using Xunit;

namespace ArmaForces.ArmaServerManager.Tests.Extensions
{
    [Trait("Category", "Unit")]
    public class ResultExtensionsTests
    {
        private readonly Fixture _fixture = new Fixture();
        
        [Fact]
        public void TapOnBoth_FailureResult_ActionInvoked()
        {
            var actionMock = new Mock<Action<Result<bool>>>();
            var result = Result.Failure<bool>(_fixture.Create<string>());

            Task<Result<bool>> taskResult = Task.FromResult(result);

            taskResult.TapOnBoth(actionMock.Object);
            
            actionMock.Verify(x => x.Invoke(result), Times.Once);
        }
        
        [Fact]
        public void TapOnBoth_SuccessResult_ActionInvoked()
        {
            var actionMock = new Mock<Action<Result<bool>>>();
            var result = Result.Success(_fixture.Create<bool>());

            Task<Result<bool>> taskResult = Task.FromResult(result);

            taskResult.TapOnBoth(actionMock.Object);
            
            actionMock.Verify(x => x.Invoke(result), Times.Once);
        }
    }
}
