using System.Net;

namespace ArmaForces.Arma.Server.Common.Errors;

public class ErrorCode : IErrorCode
{
    public HttpStatusCode? HttpStatusCode { get; } = null;
    
    public virtual int? Value => (int?) HttpStatusCode;

    public ErrorCode()
    {
    }
    
    public ErrorCode(HttpStatusCode httpStatusCode)
    {
        HttpStatusCode = httpStatusCode;
    }

    public bool Is<T>(T value) => value switch 
    {
        HttpStatusCode httpStatusCode => httpStatusCode == HttpStatusCode,
        int intValue => Value == intValue,
        _ => false
    };

    public override string ToString() => HttpStatusCode.ToString() ?? "Unknown";

    public static implicit operator ErrorCode(HttpStatusCode httpStatusCode) => new(httpStatusCode);
    
    public static bool operator ==(ErrorCode errorCode, HttpStatusCode httpStatusCode) => errorCode.HttpStatusCode == httpStatusCode;

    public static bool operator !=(ErrorCode errorCode, HttpStatusCode httpStatusCode) => !(errorCode == httpStatusCode);
}