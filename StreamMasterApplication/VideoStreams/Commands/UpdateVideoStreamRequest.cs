﻿using FluentValidation;

using StreamMasterApplication.VideoStreams.Events;

namespace StreamMasterApplication.VideoStreams.Commands;

public class UpdateVideoStreamRequestValidator : AbstractValidator<UpdateVideoStreamRequest>
{
    public UpdateVideoStreamRequestValidator()
    {
        _ = RuleFor(v => v.Id).NotNull().NotEmpty();
    }
}

public class UpdateVideoStreamRequestHandler(ILogger<UpdateVideoStreamRequest> logger, IRepositoryWrapper repository, IMapper mapper, IPublisher publisher, ISender sender, IHubContext<StreamMasterHub, IStreamMasterHub> hubContext, IMemoryCache memoryCache) : BaseMemoryRequestHandler(logger, repository, mapper, publisher, sender, hubContext, memoryCache), IRequestHandler<UpdateVideoStreamRequest, VideoStreamDto?>
{
    public async Task<VideoStreamDto?> Handle(UpdateVideoStreamRequest request, CancellationToken cancellationToken)
    {

        (VideoStreamDto? videoStream, bool updateChannelGroup) = await Repository.VideoStream.UpdateVideoStreamAsync(request, cancellationToken).ConfigureAwait(false);
        if (videoStream is not null)
        {
            await Publisher.Publish(new UpdateVideoStreamEvent(videoStream, updateChannelGroup), cancellationToken).ConfigureAwait(false);
        }


        return videoStream;
    }
}
