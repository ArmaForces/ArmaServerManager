using System;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Exceptions
{
    /// <summary>
    /// Used for failing hangfire jobs.
    /// </summary>
    public class JobFailedException : Exception
    {
        public JobFailedException(string message) : base(message)
        {
        }
    }
}
