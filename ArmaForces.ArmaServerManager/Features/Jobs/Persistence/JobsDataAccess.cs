using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Constants;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using Hangfire.LiteDB;
using LiteDB;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence
{
    internal class JobsDataAccess : IJobsDataAccess
    {
        private readonly HangfireDbContext _dbContext;

        public JobsDataAccess(HangfireDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<JobDataModel> GetJobs(ISet<JobStatus> includeStatuses)
        {
            return JobsTable
                .Query()
                .Where(x => includeStatuses.Contains(x.JobStatus))
                .ToList();
        }

        private ILiteCollection<JobDataModel> JobsTable => _dbContext.Database.GetCollection<JobDataModel>(HangfireDatabaseConstants.JobsTable);
    }
}