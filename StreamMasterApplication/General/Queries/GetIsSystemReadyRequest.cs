﻿using MediatR;

using Microsoft.Extensions.Caching.Memory;
using StreamMasterDomain.Cache;

namespace StreamMasterApplication.General.Queries;

public record GetIsSystemReadyRequest : IRequest<bool>;

internal class GetIsSystemReadyRequestHandler : IRequestHandler<GetIsSystemReadyRequest, bool>
{
    private readonly IMemoryCache _memoryCache;

    public GetIsSystemReadyRequestHandler(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<bool> Handle(GetIsSystemReadyRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_memoryCache.IsSystemReady());
    }
}
