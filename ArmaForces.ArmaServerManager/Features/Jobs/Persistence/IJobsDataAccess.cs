using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ArmaForces.ArmaServerManager.Features.Jobs.Persistence.Models;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Jobs.Persistence
{
    internal interface IJobsDataAccess
    {
        List<T> GetJobs<T>(Expression<Func<T, bool>> filterExpression) where T : JobDataModel;
        
        Result<T> GetJob<T>(int jobId) where T : JobDataModel;
    }
}