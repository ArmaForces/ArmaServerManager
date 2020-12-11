using System.Threading;
using System.Threading.Tasks;
using Arma.Server.Manager.Features.Steam.Content.DTOs;
using CSharpFunctionalExtensions;

namespace Arma.Server.Manager.Features.Steam.Content
{
    public interface IContentVerifier
    {
        Task<Result<ContentItem>> ItemIsUpToDate(ContentItem contentItem, CancellationToken cancellationToken);
    }
}
