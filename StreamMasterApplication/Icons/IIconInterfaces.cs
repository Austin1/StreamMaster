﻿using Microsoft.AspNetCore.Mvc;

using StreamMasterApplication.Icons.Commands;

using StreamMasterDomain.Dto;
using StreamMasterDomain.Pagination;

namespace StreamMasterApplication.Icons;

public interface IIconController
{
    Task<ActionResult> AutoMatchIconToStreams(AutoMatchIconToStreamsRequest request);

    Task<ActionResult<IconFileDto>> GetIcon(int Id);

    Task<ActionResult<IEnumerable<IconFileDto>>> GetIcons(IconFileParameters iconFileParameters);
    Task<ActionResult<IEnumerable<IconSimpleDto>>> GetIconsSimpleQuery();
}

public interface IIconDB
{
}

public interface IIconHub
{
    Task AutoMatchIconToStreams(AutoMatchIconToStreamsRequest request);

    Task<IconFileDto?> GetIcon(int Id);
    Task<IEnumerable<IconFileDto>> GetIcons(IconFileParameters iconFileParameters);
    Task<IEnumerable<IconSimpleDto>> GetIconsSimpleQuery();
}

public interface IIconScoped
{
}

public interface IIconTasks
{
    ValueTask BuildIconCaches(CancellationToken cancellationToken = default);

    ValueTask BuildIconsCacheFromVideoStreams(CancellationToken cancellationToken = default);

    ValueTask BuildProgIconsCacheFromEPGs(CancellationToken cancellationToken = default);

    Task ReadDirectoryLogosRequest(CancellationToken cancellationToken = default);

    ValueTask ScanDirectoryForIconFiles(CancellationToken cancellationToken = default);
}