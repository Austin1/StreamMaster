﻿using MediatR;

using StreamMasterApplication.Common.Models;
using StreamMasterApplication.StreamGroups;
using StreamMasterApplication.StreamGroups.Commands;
using StreamMasterApplication.StreamGroups.Queries;
using StreamMasterApplication.StreamGroupVideoStreams;
using StreamMasterApplication.StreamGroupVideoStreams.Commands;
using StreamMasterApplication.StreamGroupVideoStreams.Queries;
using StreamMasterApplication.VideoStreamLinks.Commands;
using StreamMasterApplication.VideoStreamLinks.Queries;

using StreamMasterDomain.Attributes;
using StreamMasterDomain.Dto;
using StreamMasterDomain.Pagination;

namespace StreamMasterApplication.Hubs;

public partial class StreamMasterHub : IStreamGroupVideoStreamHub
{
    public async Task AddVideoStreamToStreamGroup(AddVideoStreamToStreamGroupRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<VideoStreamIsReadOnly>> GetStreamGroupVideoStreamIds(GetStreamGroupVideoStreamIdsRequest request, CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<PagedResponse<VideoStreamDto>> GetStreamGroupVideoStreams(StreamGroupVideoStreamParameters Parameters, CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new GetStreamGroupVideoStreamsRequest(Parameters), cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveVideoStreamFromStreamGroup(RemoveVideoStreamFromStreamGroupRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(request, cancellationToken).ConfigureAwait(false);
    }
}