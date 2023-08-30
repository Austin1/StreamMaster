﻿using MediatR;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using StreamMasterApplication.ChannelGroups.Commands;
using StreamMasterApplication.Hubs;
using StreamMasterApplication.VideoStreams.Events;

namespace StreamMasterApplication.VideoStreams.EventHandlers;

public class CreateVideoStreamEventHandler : INotificationHandler<CreateVideoStreamEvent>
{
    private readonly IHubContext<StreamMasterHub, IStreamMasterHub> _hubContext;
    private readonly ILogger<CreateVideoStreamEventHandler> _logger;
    private readonly ISender _sender;

    public CreateVideoStreamEventHandler(
         IHubContext<StreamMasterHub, IStreamMasterHub> hubContext,
           ISender sender,
        ILogger<CreateVideoStreamEventHandler> logger
        )
    {
        _sender = sender;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Handle(CreateVideoStreamEvent notification, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.ChannelGroupsRefresh().ConfigureAwait(false);
    }
}