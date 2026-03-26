using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Common.Errors;

public interface IError : ICombine
{
    public string Message { get; init; }
    
    public IErrorCode Code { get; init; }
    
    public IError? InnerError { get; init; }
}

public interface IErrorCode
{
    public int? Value { get; }

    public bool Is<T>(T value);
}