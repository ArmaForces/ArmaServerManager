using System;
using System.Threading.Tasks;
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
