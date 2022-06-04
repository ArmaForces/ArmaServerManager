using System.Collections.Generic;
using System.Linq;
using ArmaForces.ArmaServerManager.Features.Hangfire.Models;
using ArmaForces.ArmaServerManager.Features.Hangfire.Persistence.Constants;
using ArmaForces.ArmaServerManager.Features.Hangfire.Persistence.Models;
using Hangfire.LiteDB;
using LiteDB;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Persistence
{
    internal class HangfireDataAccess : IHangfireDataAccess
    {
        private readonly HangfireDbContext _dbContext;

        public HangfireDataAccess(HangfireDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<JobDataModel> GetJobs(IEnumerable<JobStatus> includeStatuses)
        {
            return JobsTable
                .Query()
                .Where(x => includeStatuses.Contains(x.JobStatus))
                .ToList();
        }

        private ILiteCollection<JobDataModel> JobsTable => _dbContext.Database.GetCollection<JobDataModel>(HangfireDatabaseConstants.JobsTable);
    }
}