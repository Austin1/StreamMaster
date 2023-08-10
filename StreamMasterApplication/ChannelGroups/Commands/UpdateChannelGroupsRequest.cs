﻿using AutoMapper;
using AutoMapper.QueryableExtensions;

using FluentValidation;

using MediatR;

using Microsoft.Extensions.Logging;

using StreamMasterApplication.M3UFiles.Commands;
using StreamMasterApplication.VideoStreams.Events;

using StreamMasterDomain.Attributes;
using StreamMasterDomain.Dto;

namespace StreamMasterApplication.ChannelGroups.Commands;

[RequireAll]
public record UpdateChannelGroupsRequest(IEnumerable<UpdateChannelGroupRequest> ChannelGroupRequests) : IRequest<IEnumerable<ChannelGroupDto>?>
{
}

public class UpdateChannelGroupsRequestValidator : AbstractValidator<UpdateChannelGroupsRequest>
{
}

public class UpdateChannelGroupsRequestHandler : BaseMediatorRequestHandler, IRequestHandler<UpdateChannelGroupsRequest, IEnumerable<ChannelGroupDto>?>
{
    public UpdateChannelGroupsRequestHandler(ILogger<CreateM3UFileRequestHandler> logger, IRepositoryWrapper repository, IMapper mapper, IPublisher publisher, ISender sender)
        : base(logger, repository, mapper, publisher, sender) { }

    public async Task<IEnumerable<ChannelGroupDto>?> Handle(UpdateChannelGroupsRequest requests, CancellationToken cancellationToken)
    {
        List<VideoStreamDto> results = new();
        List<ChannelGroupDto> cgResults = new();

        foreach (UpdateChannelGroupRequest request in requests.ChannelGroupRequests)
        {
            ChannelGroup? channelGroup = await Repository.ChannelGroup.GetChannelGroupByNameAsync(request.GroupName.ToLower()).ConfigureAwait(false);

            if (channelGroup == null)
            {
                continue;
            }

            if (request.Rank != null)
            {
                channelGroup.Rank = (int)request.Rank;
            }

            bool isChanged = false;

            if (request.IsHidden != null)
            {
                channelGroup.IsHidden = (bool)request.IsHidden;

                await Repository.VideoStream.SetGroupVisibleByGroupName(channelGroup.Name, (bool)request.IsHidden, cancellationToken).ConfigureAwait(false);

                isChanged = true;
            }

            if (!string.IsNullOrEmpty(request.NewGroupName))
            {
                await Repository.VideoStream.SetGroupNameByGroupName(channelGroup.Name, request.NewGroupName, cancellationToken).ConfigureAwait(false);

                channelGroup.Name = request.NewGroupName;
                isChanged = true;
            }

            Repository.ChannelGroup.UpdateChannelGroup(channelGroup);
            await Repository.SaveAsync().ConfigureAwait(false);

            cgResults.Add(Mapper.Map<ChannelGroupDto>(channelGroup));

            if (isChanged)
            {
                var re = await Repository.VideoStream.GetVideoStreamsChannelGroupName(channelGroup.Name).ConfigureAwait(false);
                var toadd = Mapper.Map<List<VideoStreamDto>>(re);
                results.AddRange(toadd);
            }
        }

        await Publisher.Publish(new UpdateChannelGroupsEvent(cgResults), cancellationToken).ConfigureAwait(false);

        if (results.Any())
        {
            await Publisher.Publish(new UpdateVideoStreamsEvent(results), cancellationToken).ConfigureAwait(false);
        }

        return cgResults;
    }
}