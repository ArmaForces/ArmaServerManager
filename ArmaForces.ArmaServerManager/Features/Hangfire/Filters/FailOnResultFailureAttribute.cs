using ArmaForces.ArmaServerManager.Features.Hangfire.Exceptions;
using CSharpFunctionalExtensions;
using Hangfire.Common;
using Hangfire.States;

namespace ArmaForces.ArmaServerManager.Features.Hangfire.Filters
{
    /// <summary>
    /// Moves job to failed state when failure <see cref="Result{T}"/> is returned.
    /// </summary>
    public class FailOnResultFailureAttribute : JobFilterAttribute, IElectStateFilter
    {
        /// <inheritdoc />
        public void OnStateElection(ElectStateContext context)
        {
            var contextCandidateState = context.CandidateState as SucceededState;
            if (contextCandidateState?.Result is Result result)
            {
                result.OnFailure(error => SetFailedState(context, error));
            }
        }

        /// <summary>
        /// Sets job status to <see cref="FailedState"/>.
        /// Creates new <see cref="JobFailedException"/> with <paramref name="error"/> message.
        /// </summary>
        /// <param name="context">Job context.</param>
        /// <param name="error">Error message for failed state.</param>
        private static void SetFailedState(ElectStateContext context, string error)
        {
            var jobFailedException = new JobFailedException(error);
            context.CandidateState = new FailedState(jobFailedException);
        }
    }
}
