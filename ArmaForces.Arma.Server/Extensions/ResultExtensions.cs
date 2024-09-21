using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace ArmaForces.Arma.Server.Extensions;

public static class ResultExtensions
{
    public static async Task<UnitResult<TError>> Combine<TError>(this IEnumerable<Task<UnitResult<TError>>> tasks)
        where TError: ICombine
    {
        IEnumerable<UnitResult<TError>> results = await Task.WhenAll(tasks);
        return results.Combine();
    }
}