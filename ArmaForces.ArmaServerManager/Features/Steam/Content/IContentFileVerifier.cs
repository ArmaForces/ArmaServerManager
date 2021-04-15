using System.Collections.Generic;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using BytexDigital.Steam.ContentDelivery.Models;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    internal interface IContentFileVerifier
    {
        Result<ContentItem> EnsureDirectoryExists(ContentItem contentItem);

        void RemoveRedundantFiles(string directory, Manifest manifest);

        void RemoveRedundantFiles(string directory, IReadOnlyCollection<string> expectedFilesAndDirectories);

        bool FileIsUpToDate(string directory, ManifestFile file);
    }
}
