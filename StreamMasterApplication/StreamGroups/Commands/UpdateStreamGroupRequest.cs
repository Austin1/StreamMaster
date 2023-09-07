﻿using FluentValidation;

using StreamMasterApplication.VideoStreams.Events;

namespace StreamMasterApplication.StreamGroups.Commands;

public class UpdateStreamGroupRequestValidator : AbstractValidator<UpdateStreamGroupRequest>
{
    public UpdateStreamGroupRequestValidator()
    {
        _ = RuleFor(v => v.StreamGroupId)
           .NotNull()
           .GreaterThanOrEqualTo(0);
    }
}

public class UpdateStreamGroupRequestHandler : BaseMediatorRequestHandler, IRequestHandler<UpdateStreamGroupRequest, StreamGroupDto?>
{

    public UpdateStreamGroupRequestHandler(ILogger<UpdateStreamGroupRequest> logger, IRepositoryWrapper repository, IMapper mapper, IPublisher publisher, ISender sender, IHubContext<StreamMasterHub, IStreamMasterHub> hubContext)
  : base(logger, repository, mapper, publisher, sender, hubContext) { }


    public async Task<StreamGroupDto?> Handle(UpdateStreamGroupRequest request, CancellationToken cancellationToken)
    {
        if (request.StreamGroupId < 1)
        {
            return null;
        }

        StreamGroupDto? streamGroup = await Repository.StreamGroup.UpdateStreamGroupAsync(request, cancellationToken).ConfigureAwait(false);
        if (streamGroup is not null)
        {
            //var streamGroup = await _context.GetStreamGroupDto(ret.Id, url, cancellationToken).ConfigureAwait(false);
            if (streamGroup is not null && streamGroup.ChildVideoStreams.Any())
            {
                await Publisher.Publish(new UpdateVideoStreamsEvent(streamGroup.ChildVideoStreams), cancellationToken).ConfigureAwait(false);
            }
            await Publisher.Publish(new StreamGroupUpdateEvent(), cancellationToken).ConfigureAwait(false);
        }

        return streamGroup;
    }
}
