using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Hangfire.Persistence.Models;
using Hangfire.LiteDB;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Persistence
{
    internal class HangfireDataAccess : IHangfireDataAccess
    {
        private readonly HangfireDbContext _dbContext;

        public HangfireDataAccess(HangfireDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<JobDataModel> GetQueuedJobs()
        {
            var jobs = _dbContext.Database.GetCollection<JobDataModel>("hangfire_job")
                .Query()
                .Where(x => x.JobStatus == JobStatus.Awaiting || x.JobStatus == JobStatus.Enqueued ||
                            x.JobStatus == JobStatus.Scheduled)
                .ToList();

            return jobs;
        }
    }
}