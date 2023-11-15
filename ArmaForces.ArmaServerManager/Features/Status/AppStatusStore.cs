using System;
using ArmaForces.ArmaServerManager.Api.Status.DTOs;
using ArmaForces.ArmaServerManager.Features.Status.Models;

namespace ArmaForces.ArmaServerManager.Features.Status;

/// <summary>
/// TODO: Consider doing this in a better way. For now it's fine.
/// </summary>
public class AppStatusStore : IAppStatusStore
{
    public AppStatusDetails? StatusDetails { get; private set; }
    
    public IDisposable SetAppStatus(AppStatus appStatus, string? longStatus = null)
    {
        var previousStatus = StatusDetails;
        
        StatusDetails = new AppStatusDetails
        {
            Status = appStatus,
            LongStatus = longStatus
        };

        return new AppStatusDisposable(this, previousStatus);
    }

    public void ClearAppStatus()
    {
        StatusDetails = null;
    }

    private class AppStatusDisposable : IDisposable
    {
        private readonly AppStatusStore _appStatusStore;
        private readonly AppStatusDetails? _previousStatus;

        public AppStatusDisposable(AppStatusStore appStatusStore, AppStatusDetails? previousStatus)
        {
            _appStatusStore = appStatusStore;
            _previousStatus = previousStatus;
        }
        
        public void Dispose()
        {
            _appStatusStore.StatusDetails = _previousStatus;
        }
    }
}

public interface IAppStatusStore
{
    AppStatusDetails? StatusDetails { get; }

    IDisposable SetAppStatus(AppStatus appStatus, string? longStatus = null);
    
    void ClearAppStatus();
}