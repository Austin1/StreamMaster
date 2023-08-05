﻿using AutoMapper;

using FluentValidation;

using MediatR;

using Microsoft.Extensions.Logging;

using StreamMasterApplication.M3UFiles.Commands;
using StreamMasterApplication.VideoStreams.Events;

namespace StreamMasterApplication.VideoStreams.Commands;

public record DeleteVideoStreamRequest(string Id) : IRequest<string?>
{
}

public class DeleteVideoStreamRequestValidator : AbstractValidator<DeleteVideoStreamRequest>
{
    public DeleteVideoStreamRequestValidator()
    {
        _ = RuleFor(v => v.Id).NotNull().NotEmpty();
    }
}

public class DeleteVideoStreamRequestHandler : BaseMediatorRequestHandler, IRequestHandler<DeleteVideoStreamRequest, string?>
{

    public DeleteVideoStreamRequestHandler(ILogger<CreateM3UFileRequestHandler> logger, IRepositoryWrapper repository, IMapper mapper, IPublisher publisher, ISender sender)
        : base(logger, repository, mapper, publisher, sender) { }

    public async Task<string?> Handle(DeleteVideoStreamRequest request, CancellationToken cancellationToken)
    {
        var videostream = await Repository.VideoStream.GetVideoStreamByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (videostream == null)
        {
            return null;
        }

        Repository.VideoStream.Delete(videostream);
        await Repository.SaveAsync().ConfigureAwait(false);


        await Publisher.Publish(new DeleteVideoStreamEvent(request.Id), cancellationToken).ConfigureAwait(false);
        return request.Id;

    }
}
