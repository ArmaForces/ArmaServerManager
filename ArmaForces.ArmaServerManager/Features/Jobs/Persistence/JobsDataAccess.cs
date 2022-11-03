using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Constants;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;
using Hangfire.LiteDB;
using LiteDB;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence
{
    internal class JobsDataAccess : IJobsDataAccess
    {
        private const string JobDoesntExistError = "Job doesn't exist.";
        
        private readonly HangfireDbContext _dbContext;

        public JobsDataAccess(HangfireDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<T> GetJobs<T>(Expression<Func<T, bool>> filterExpression) where T : JobDataModel
            => JobsTable<T>()
                .Query()
                .Where(filterExpression)
                .ToList();

        public Result<T> GetJob<T>(int jobId) where T : JobDataModel
            => JobsTable<T>()
                .Query()
                .Where(x => x.Id == jobId)
                .SingleOrDefault() 
               ?? Result.Failure<T>(JobDoesntExistError);

        private ILiteCollection<T> JobsTable<T>() where T : JobDataModel
            => _dbContext.Database.GetCollection<T>(HangfireDatabaseConstants.JobsTable);
    }
}