﻿namespace StreamMasterApplication.ChannelGroups.Queries;

public record GetChannelGroupsFromVideoStreamIds(IEnumerable<string> VideoStreamIds) : IRequest<List<ChannelGroupDto>>;

internal class GetChannelGroupsFromVideoStreamIdsHandler : BaseMediatorRequestHandler, IRequestHandler<GetChannelGroupsFromVideoStreamIds, List<ChannelGroupDto>>
{

    public GetChannelGroupsFromVideoStreamIdsHandler(ILogger<GetChannelGroupsFromVideoStreamIds> logger, IRepositoryWrapper repository, IMapper mapper, IPublisher publisher, ISender sender, IHubContext<StreamMasterHub, IStreamMasterHub> hubContext)
 : base(logger, repository, mapper, publisher, sender, hubContext) { }
    public async Task<List<ChannelGroupDto>> Handle(GetChannelGroupsFromVideoStreamIds request, CancellationToken cancellationToken)
    {
        List<ChannelGroupDto> res = await Repository.ChannelGroup.GetChannelGroupsFromVideoStreamIds(request.VideoStreamIds, cancellationToken).ConfigureAwait(false);
        return res;
    }
}