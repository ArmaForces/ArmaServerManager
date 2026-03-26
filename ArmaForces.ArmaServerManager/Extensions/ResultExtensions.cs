using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Common.Errors;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Extensions
{
    internal static class ResultExtensions
    {
        public static async Task TapOnBoth<T>(this Task<Result<T>> taskResult, Action<Result<T>> action)
        {
            action(await taskResult);
        }
    }
}
