﻿using FluentValidation;

namespace StreamMasterApplication.StreamGroupVideoStreams.Commands;

[RequireAll]
public record SyncVideoStreamToStreamGroupRequest(int StreamGroupId, string VideoStreamId) : IRequest { }

public class SyncVideoStreamToStreamGroupRequestValidator : AbstractValidator<SyncVideoStreamToStreamGroupRequest>
{
    public SyncVideoStreamToStreamGroupRequestValidator()
    {
        _ = RuleFor(v => v.StreamGroupId)
           .NotNull()
           .GreaterThanOrEqualTo(0);
    }
}

public class SyncVideoStreamToStreamGroupRequestHandler(ILogger<SyncVideoStreamToStreamGroupRequest> logger, IRepositoryWrapper repository, IMapper mapper, IPublisher publisher, ISender sender, IHubContext<StreamMasterHub, IStreamMasterHub> hubContext) : BaseMediatorRequestHandler(logger, repository, mapper, publisher, sender, hubContext), IRequestHandler<SyncVideoStreamToStreamGroupRequest>
{
    public async Task Handle(SyncVideoStreamToStreamGroupRequest request, CancellationToken cancellationToken)
    {
        if (request.StreamGroupId < 1)
        {
            return;
        }

        StreamGroupDto? ret = await Repository.StreamGroupVideoStream.SyncVideoStreamToStreamGroup(request.StreamGroupId, request.VideoStreamId, cancellationToken).ConfigureAwait(false);
        if (ret != null)
        {
            await HubContext.Clients.All.StreamGroupsRefresh([ret]).ConfigureAwait(false);
        }
    }
}
