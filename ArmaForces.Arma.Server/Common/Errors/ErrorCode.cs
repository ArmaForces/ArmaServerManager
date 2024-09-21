using System.Net;

namespace ArmaForces.Arma.Server.Common.Errors;

public struct ErrorCode : IErrorCode
{
    public ManagerErrorCode? ManagerErrorCode { get; } = null;
    public HttpStatusCode? HttpStatusCode { get; } = null;
    
    public int? Value => (int?) ManagerErrorCode ?? (int?) HttpStatusCode;

    public ErrorCode(ManagerErrorCode managerErrorCode)
    {
        ManagerErrorCode = managerErrorCode;
    }
    
    public ErrorCode(HttpStatusCode httpStatusCode)
    {
        HttpStatusCode = httpStatusCode;
    }

    public bool Is<T>(T value) => value switch 
    {
        ManagerErrorCode managerCode => managerCode == ManagerErrorCode,
        HttpStatusCode httpStatusCode => httpStatusCode == HttpStatusCode,
        int intValue => Value == intValue,
        _ => false
    };

    public override string ToString() => ManagerErrorCode.ToString() ?? HttpStatusCode.ToString() ?? "Unknown";

    public static implicit operator ErrorCode(ManagerErrorCode httpStatusCode) => new(httpStatusCode);
    public static implicit operator ErrorCode(HttpStatusCode httpStatusCode) => new(httpStatusCode);
    
    public static bool operator ==(ErrorCode errorCode, HttpStatusCode httpStatusCode) => errorCode.HttpStatusCode == httpStatusCode;

    public static bool operator !=(ErrorCode errorCode, HttpStatusCode httpStatusCode) => !(errorCode == httpStatusCode);
    
    public static bool operator ==(ErrorCode errorCode, ManagerErrorCode managerErrorCode) => errorCode.ManagerErrorCode == managerErrorCode;

    public static bool operator !=(ErrorCode errorCode, ManagerErrorCode managerErrorCode) => !(errorCode == managerErrorCode);
}