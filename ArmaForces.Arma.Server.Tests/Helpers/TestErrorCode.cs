using ArmaForces.Arma.Server.Common.Errors;

namespace ArmaForces.Arma.Server.Tests.Helpers;

public class TestErrorCode : IErrorCode
{
    public int? Value { get; init;  }

    public bool Is<T>(T value)
    {
        throw new System.NotImplementedException();
    }

    public static ErrorCode ServerAlreadyRunning => new();
}