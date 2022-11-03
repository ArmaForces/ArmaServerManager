using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Jobs.Models;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Constants;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;
using Hangfire.LiteDB;
using Hangfire.Storage;
using LiteDB;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence
{
    internal class JobsDataAccess : IJobsDataAccess
    {
        private readonly IStorageConnection _storageConnection;
        private readonly HangfireDbContext _dbContext;

        public JobsDataAccess(
            IStorageConnection storageConnection,
            HangfireDbContext dbContext)
        {
            _storageConnection = storageConnection;
            _dbContext = dbContext;
        }

        public List<JobDataModel> GetJobs(ISet<JobStatus> includeStatuses)
        {
            return JobsTable
                .Query()
                .Where(x => includeStatuses.Contains(x.JobStatus))
                .ToList();
        }

        public Result<JobDataModel> GetJob(int jobId)
            => JobsTable
                .Query()
                .Where(x => x.Id == jobId)
                .SingleOrDefault() 
               ?? Result.Failure<JobDataModel>("Job doesn't exist.");

        private ILiteCollection<JobDataModel> JobsTable
            => _dbContext.Database.GetCollection<JobDataModel>(HangfireDatabaseConstants.JobsTable);
    }
}