namespace ArmaForces.ArmaServerManager.Features.Jobs.Models
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
