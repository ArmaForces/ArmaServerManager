using System;
using System.Net;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Common.Errors;

/// <summary>
/// Represents details of an error.
/// </summary>
public record Error: ICombine, IError
{
    public string Message { get; init; }
    public IErrorCode Code { get; init; }
    public IError InnerError { get; init; }

    /// <summary>
    /// Creates error instance.
    /// </summary>
    /// <param name="message">Message describing error details.</param>
    /// <param name="code">Code of the error.</param>
    public Error(string message, ErrorCode code)
    {
        Message = message;
        Code = code;
    }

    public Error(string message, HttpStatusCode statusCode)
        : this(message, new ErrorCode(statusCode)) { }
    
    public Error(string message, ManagerErrorCode managerErrorCode)
        : this(message, new ErrorCode(managerErrorCode)) { }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    // public static implicit operator UnitResult<Error>(Error error) => UnitResult.Failure(error);
    
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