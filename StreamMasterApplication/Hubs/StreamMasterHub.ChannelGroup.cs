﻿using StreamMasterApplication.ChannelGroups;
using StreamMasterApplication.ChannelGroups.Commands;
using StreamMasterApplication.ChannelGroups.Queries;
using StreamMasterApplication.VideoStreams.Queries;

using StreamMasterDomain.Dto;
using StreamMasterDomain.Pagination;

namespace StreamMasterApplication.Hubs;

public partial class StreamMasterHub : IChannelGroupHub
{
    public async Task CreateChannelGroup(CreateChannelGroupRequest request)
    {
        await _mediator.Send(request).ConfigureAwait(false);
    }

    public async Task DeleteChannelGroup(DeleteChannelGroupRequest request)
    {
        await _mediator.Send(request).ConfigureAwait(false);
    }

    public async Task<ChannelGroupDto?> GetChannelGroup(int id)
    {
        return await _mediator.Send(new GetChannelGroup(id)).ConfigureAwait(false);
    }

    public async Task<PagedResponse<ChannelGroupDto>> GetChannelGroups(ChannelGroupParameters channelGroupParameters)
    {
        var ret = await _mediator.Send(new GetChannelGroupsQuery(channelGroupParameters)).ConfigureAwait(false);
        return ret;
    }

    public async Task SetChannelGroupsVisible(SetChannelGroupsVisibleRequest request)
    {
        await _mediator.Send(request).ConfigureAwait(false);
    }

    public async Task UpdateChannelGroup(UpdateChannelGroupRequest request)
    {
        await _mediator.Send(request).ConfigureAwait(false);
    }

    public async Task UpdateChannelGroups(UpdateChannelGroupsRequest request)
    {
        await _mediator.Send(request).ConfigureAwait(false);
    }
}