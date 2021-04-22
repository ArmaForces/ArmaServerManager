namespace ArmaForces.ArmaServerManager.Features.Hangfire
{
    public enum JobStatus
    {
        Scheduled,

        Awaiting,

        Enqueued,

        Processing,
        
        Succeeded,

        Failed,

        Deleted
    }
}
