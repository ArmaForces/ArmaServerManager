using System;
using System.Net;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Common.Errors;

/// <summary>
/// Represents details of an error.
/// </summary>
public record Error: IError
{
    public string Message { get; init; }
    public IErrorCode Code { get; init; }
    public IError? InnerError { get; init; }

    /// <summary>
    /// Creates error instance.
    /// </summary>
    /// <param name="message">Message describing error details.</param>
    /// <param name="code">Code of the error.</param>
    /// <param name="innerError">Optional inner error.</param>
    public Error(string message, IErrorCode code, IError? innerError = null)
    {
        Message = message;
        Code = code;
        InnerError = innerError;
    }

    public Error(string message, HttpStatusCode statusCode)
        : this(message, new ErrorCode(statusCode)) { }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static implicit operator UnitResult<IError>(Error error) => UnitResult.Failure<IError>(error);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ICombine Combine(ICombine value)
    {
        throw new NotImplementedException();
    }
}