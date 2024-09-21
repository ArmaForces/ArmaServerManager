namespace ArmaForces.Arma.Server.Common.Errors;

/// <summary>
/// Error codes possible within application.
/// </summary>
/// TODO: Move this to Manager project and refactor accordingly. For now, disregard obvious problem.
public enum ManagerErrorCode
{
    /// <summary>
    /// Unexpected failure.
    /// </summary>
    InternalServerError,
    
    /// <summary>
    /// Modset doesn't exist.
    /// </summary>
    ModsetNotFound,
    
    JobContinuationCreationFailed,
    
    ParseError,
    JobDeletionFailed,
    SimilarJobFound,
    JobEnqueueFailed,
    JobScheduleFailed,
    MissionNotFound,
    ModsDownloadFailed,
    ServerNotEmpty,
    ProcessNotRunning,
    ProcessStartFailed,
    ServerStopped,
    KeyNotFound,
    ServerNotFound,
    JobNotFound,
    ArgumentError
}