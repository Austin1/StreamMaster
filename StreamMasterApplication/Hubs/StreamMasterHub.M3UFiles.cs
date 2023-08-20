﻿using StreamMasterApplication.M3UFiles;
using StreamMasterApplication.M3UFiles.Commands;
using StreamMasterApplication.M3UFiles.Queries;

using StreamMasterDomain.Dto;
using StreamMasterDomain.Pagination;

namespace StreamMasterApplication.Hubs;

public partial class StreamMasterHub : IM3UFileHub
{
    public async Task CreateM3UFile(CreateM3UFileRequest request)
    {
        await _mediator.Send(request).ConfigureAwait(false);
    }

    public async Task ChangeM3UFileName(ChangeM3UFileNameRequest request)
    {
        await _mediator.Send(request).ConfigureAwait(false);
    }

    public async Task DeleteM3UFile(DeleteM3UFileRequest request)
    {
        await _mediator.Send(request).ConfigureAwait(false);
    }

    public async Task ProcessM3UFile(ProcessM3UFileRequest request)
    {
        await _mediator.Send(request).ConfigureAwait(false);
    }

    public async Task RefreshM3UFile(RefreshM3UFileRequest request)
    {
        await _mediator.Send(request).ConfigureAwait(false);
    }

    public async Task ScanDirectoryForM3UFiles()
    {
        await _mediator.Send(new ScanDirectoryForM3UFilesRequest()).ConfigureAwait(false);
    }

    public async Task UpdateM3UFile(UpdateM3UFileRequest request)
    {
        await _mediator.Send(request).ConfigureAwait(false);
    }

    public async Task<PagedResponse<M3UFileDto>> GetM3UFiles(M3UFileParameters m3uFileParameters)
    {
        PagedResponse<M3UFileDto> data = await _mediator.Send(new GetM3UFilesQuery(m3uFileParameters)).ConfigureAwait(false);
        return data;
    }

    public async Task<M3UFileDto?> GetM3UFile(int id)
    {
        M3UFileDto? data = await _mediator.Send(new GetM3UFileByIdQuery(id)).ConfigureAwait(false);

        return data;
    }

    public async Task<List<string>> GetM3UFileNames()
    {
        return await _mediator.Send(new GetM3UFileNamesQuery()).ConfigureAwait(false);

    }
}