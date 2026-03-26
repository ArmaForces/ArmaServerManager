using System.Net;
using ArmaForces.Arma.Server.Common.Errors;

namespace ArmaForces.ArmaServerManager.Common.Errors;

public static class ManagerErrors
{
    public static IError MissionNotFound(string missionTitle)
        => new Error($"Mission {missionTitle} not found.", new ManagerErrorCode2(ManagerErrorCode.MissionNotFound));
}

public class ManagerErrorCode2 : ErrorCode
{
    public ManagerErrorCode? ManagerErrorCode { get; } = null;
    
    public ManagerErrorCode2(ManagerErrorCode managerErrorCode)
    {
        ManagerErrorCode = managerErrorCode;
    }

    public override int? Value => (int?) ManagerErrorCode ?? base.Value;

    public bool Is<T>(T value) => value switch
    {
        ManagerErrorCode managerCode => managerCode == ManagerErrorCode,
        _ => base.Is(value)
    };
    
    
    public static implicit operator ManagerErrorCode2(ManagerErrorCode httpStatusCode) => new(httpStatusCode);
    
    public static bool operator ==(ManagerErrorCode2 errorCode, ManagerErrorCode managerErrorCode) => errorCode.ManagerErrorCode == managerErrorCode;
    
    public static bool operator !=(ManagerErrorCode2 errorCode, ManagerErrorCode managerErrorCode) => !(errorCode == managerErrorCode);
}