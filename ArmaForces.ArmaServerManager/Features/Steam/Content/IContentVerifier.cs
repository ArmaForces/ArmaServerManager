﻿using System.Threading;
using System.Threading.Tasks;
using ArmaForces.ArmaServerManager.Features.Steam.Content.DTOs;
using CSharpFunctionalExtensions;

namespace ArmaForces.ArmaServerManager.Features.Steam.Content
{
    internal interface IContentVerifier
    {
        Task<Result<ContentItem>> ItemIsUpToDate(ContentItem contentItem, CancellationToken cancellationToken);
    }
}
