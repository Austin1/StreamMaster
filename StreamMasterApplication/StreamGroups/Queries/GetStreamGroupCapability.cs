﻿using FluentValidation;

using Microsoft.AspNetCore.Http;

using StreamMasterApplication.Common.Models;

using System.Xml.Serialization;

using static StreamMasterDomain.Common.GetStreamGroupEPGHandler;

namespace StreamMasterApplication.StreamGroups.Queries;

[RequireAll]
public record GetStreamGroupCapability(int StreamGroupNumber) : IRequest<string>;

public class GetStreamGroupCapabilityValidator : AbstractValidator<GetStreamGroupCapability>
{
    public GetStreamGroupCapabilityValidator()
    {
        _ = RuleFor(v => v.StreamGroupNumber)
            .NotNull().GreaterThanOrEqualTo(0);
    }
}

public class GetStreamGroupCapabilityHandler : BaseMediatorRequestHandler, IRequestHandler<GetStreamGroupCapability, string>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public GetStreamGroupCapabilityHandler(IHttpContextAccessor httpContextAccessor, ILogger<GetStreamGroupCapability> logger, IRepositoryWrapper repository, IMapper mapper, IPublisher publisher, ISender sender, IHubContext<StreamMasterHub, IStreamMasterHub> hubContext)
: base(logger, repository, mapper, publisher, sender, hubContext) { _httpContextAccessor = httpContextAccessor; }


    public async Task<string> Handle(GetStreamGroupCapability request, CancellationToken cancellationToken)
    {

        if (request.StreamGroupNumber > 0)
        {
            StreamGroupDto? streamGroup = await Repository.StreamGroup.GetStreamGroupDtoByStreamGroupNumber(request.StreamGroupNumber, cancellationToken).ConfigureAwait(false);
            if (streamGroup == null)
            {
                return "";
            }
        }
        Setting settings = FileUtil.GetSetting();

        Capability capability = new(GetUrl(), $"{settings.DeviceID}-{request.StreamGroupNumber}");

        using Utf8StringWriter textWriter = new();
        XmlSerializer serializer = new(typeof(Capability));
        serializer.Serialize(textWriter, capability);

        return textWriter.ToString();
    }

    private string GetUrl()
    {
        HttpRequest request = _httpContextAccessor.HttpContext.Request;
        string scheme = request.Scheme;
        HostString host = request.Host;
        PathString path = request.Path;
        path = path.ToString().Replace("/capability", "");
        path = path.ToString().Replace("/device.xml", "");
        string url = $"{scheme}://{host}{path}";

        return url;
    }
}
