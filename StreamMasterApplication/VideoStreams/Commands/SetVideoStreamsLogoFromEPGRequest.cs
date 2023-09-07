﻿using StreamMasterApplication.VideoStreams.Events;

namespace StreamMasterApplication.VideoStreams.Commands;

public record SetVideoStreamsLogoFromEPGRequest(List<string> Ids, string? OrderBy) : IRequest<List<VideoStreamDto>> { }

public class SetVideoStreamsLogoFromEPGRequestHandler : BaseMediatorRequestHandler, IRequestHandler<SetVideoStreamsLogoFromEPGRequest, List<VideoStreamDto>>
{

    public SetVideoStreamsLogoFromEPGRequestHandler(ILogger<SetVideoStreamsLogoFromEPGRequest> logger, IRepositoryWrapper repository, IMapper mapper, IPublisher publisher, ISender sender, IHubContext<StreamMasterHub, IStreamMasterHub> hubContext)
: base(logger, repository, mapper, publisher, sender, hubContext) { }

    public async Task<List<VideoStreamDto>> Handle(SetVideoStreamsLogoFromEPGRequest request, CancellationToken cancellationToken)
    {
        List<VideoStreamDto> results = await Repository.VideoStream.SetVideoStreamsLogoFromEPGFromIds(request.Ids, cancellationToken).ConfigureAwait(false);

        if (results.Any())
        {
            await Publisher.Publish(new UpdateVideoStreamsEvent(results), cancellationToken).ConfigureAwait(false);
        }

        return results;
    }
}
