using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmaForces.Arma.Server.Features.Modsets;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Services;

/// <summary>
///  Performs detailed verifications of installed mods.
/// </summary>
public interface IModsVerificationService
{
    /// <summary>
    /// Runs detailed verification of given <paramref name="modset"/>.
    /// </summary>
    /// <param name="modset">Modset with mods to verify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Successful result if all mods were verified correctly.</returns>
    Task<Result> VerifyModset(Modset modset, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves modset with given <paramref name="modsetName"/> and runs a detailed verification.
    /// </summary>
    /// <param name="modsetName">Name of the modset to retrieve and verify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Successful result if all mods were verified correctly.</returns>
    Task<Result> VerifyModset(string modsetName, CancellationToken cancellationToken);
}