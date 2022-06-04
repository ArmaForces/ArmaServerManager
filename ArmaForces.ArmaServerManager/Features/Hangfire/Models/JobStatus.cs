namespace ArmaForces.ArmaServerManager.Features.Hangfire.Models
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
