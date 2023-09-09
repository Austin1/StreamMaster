﻿using AutoMapper;

using MediatR;

using Microsoft.EntityFrameworkCore;

using StreamMasterApplication.Common.Attributes;
using StreamMasterApplication.StreamGroups.Queries;

using StreamMasterDomain.Dto;
using StreamMasterDomain.Pagination;
using StreamMasterDomain.Repository;

using System.Linq.Dynamic.Core;

namespace StreamMasterInfrastructureEF.Repositories;

public class StreamGroupVideoStreamRepository(RepositoryContext repositoryContext, IRepositoryWrapper repository, IMapper mapper, ISender sender) : RepositoryBase<StreamGroupVideoStream, StreamGroupVideoStream>(repositoryContext), IStreamGroupVideoStreamRepository
{
    public async Task AddStreamGroupVideoStreams(int StreamGroupId, List<string> toAdd, bool IsReadOnly, CancellationToken cancellationToken)
    {
        try
        {
            StreamGroup? streamGroup = await repository.StreamGroup.GetStreamGroupByIdAsync(StreamGroupId).ConfigureAwait(false);

            if (streamGroup == null)
            {
                return;
            }

            List<string> existing = await FindByCondition(a => a.StreamGroupId == StreamGroupId).Select(a => a.ChildVideoStreamId).ToListAsync(cancellationToken).ConfigureAwait(false);

            List<string> toRun = toAdd.Except(existing).ToList();
            if (!toRun.Any())
            {
                return;
            }

            List<StreamGroupVideoStream> streamGroupVideoStreams = toRun.Select(videoStreamId => new StreamGroupVideoStream
            {
                StreamGroupId = StreamGroupId,
                ChildVideoStreamId = videoStreamId,
                IsReadOnly = IsReadOnly
            }).ToList();

            BulkInsert(streamGroupVideoStreams);

            await UpdateRanks(StreamGroupId, cancellationToken);

        }
        catch (Exception)
        {
        }
    }

    [LogExecutionTimeAspect]
    public async Task<PagedResponse<VideoStreamDto>> GetStreamGroupVideoStreams(StreamGroupVideoStreamParameters Parameters, CancellationToken cancellationToken = default)
    {

        if (Parameters.StreamGroupId == 0 || Parameters == null)
        {
            return Parameters.CreateEmptyPagedResponse<VideoStreamDto>();
        }

        IQueryable<VideoStream> childQ = RepositoryContext.StreamGroupVideoStreams
        .Include(a => a.ChildVideoStream)
        .Where(a => a.StreamGroupId == Parameters.StreamGroupId)
        .Select(a => a.ChildVideoStream);

        PagedResponse<VideoStreamDto> pagedResponse = await childQ.GetPagedResponseWithFilterAsync<VideoStream, VideoStreamDto>(Parameters.JSONFiltersString, Parameters.OrderBy, Parameters.PageNumber, Parameters.PageSize, mapper).ConfigureAwait(false);

        List<string> streamIds = pagedResponse.Data.Select(a => a.Id).ToList();

        List<StreamGroupVideoStream> existinglinks = RepositoryContext.StreamGroupVideoStreams
            .Where(a => a.StreamGroupId == Parameters.StreamGroupId && streamIds.Contains(a.ChildVideoStreamId) && a.IsReadOnly == true).ToList();

        foreach (VideoStreamDto item in pagedResponse.Data)
        {
            if (existinglinks.Any(a => a.ChildVideoStreamId == item.Id))
            {
                item.IsReadOnly = true;
            }
        }

        return pagedResponse;
    }
    public async Task<StreamGroupDto?> AddVideoStreamToStreamGroup(int StreamGroupId, string VideoStreamId, CancellationToken cancellationToken)
    {
        try
        {
            StreamGroup? streamGroup = await repository.StreamGroup.GetStreamGroupByIdAsync(StreamGroupId).ConfigureAwait(false);

            if (streamGroup == null)
            {
                return null;
            }

            if (FindByCondition(a => a.StreamGroupId == StreamGroupId && a.ChildVideoStreamId == VideoStreamId).Any())
            {
                return null;
            }

            Create(new StreamGroupVideoStream { StreamGroupId = StreamGroupId, ChildVideoStreamId = VideoStreamId, Rank = Count() });
            await UpdateRanks(StreamGroupId, cancellationToken);
            return await sender.Send(new GetStreamGroup(StreamGroupId), cancellationToken);
        }
        catch (Exception)
        {
        }
        return null;
    }

    public async Task<List<VideoStreamIsReadOnly>> GetStreamGroupVideoStreamIds(int id, CancellationToken cancellationToken = default)
    {
        if (id == 0)
        {
            return new();
        }

        List<VideoStreamIsReadOnly> ret = await RepositoryContext.StreamGroupVideoStreams.Where(a => a.StreamGroupId == id)
            .AsNoTracking()
            .Select(a => a.ChildVideoStreamId)
            .Select(a => new VideoStreamIsReadOnly { VideoStreamId = a, IsReadOnly = false }).ToListAsync(cancellationToken: cancellationToken);

        return ret.OrderBy(a => a.Rank).ToList();
    }

    public async Task RemoveStreamGroupVideoStreams(int StreamGroupId, IEnumerable<string> toRemove, CancellationToken cancellationToken)
    {
        StreamGroup? streamGroup = await repository.StreamGroup.GetStreamGroupByIdAsync(StreamGroupId).ConfigureAwait(false);

        if (streamGroup == null)
        {
            return;
        }

        IQueryable<StreamGroupVideoStream> toDelete = FindByCondition(a => a.StreamGroupId == StreamGroupId && toRemove.Contains(a.ChildVideoStreamId));
        //await RepositoryContext.BulkDeleteAsync(toDelete, cancellationToken: cancellationToken).ConfigureAwait(false);

        RepositoryContext.StreamGroupVideoStreams.RemoveRange(toDelete);
        await repository.SaveAsync().ConfigureAwait(false);

        await UpdateRanks(StreamGroupId, cancellationToken).ConfigureAwait(false);
    }

    public async Task SetVideoStreamRanks(int StreamGroupId, List<VideoStreamIDRank> videoStreamIDRanks, CancellationToken cancellationToken)
    {
        StreamGroup? streamGroup = await repository.StreamGroup.GetStreamGroupByIdAsync(StreamGroupId).ConfigureAwait(false);

        if (streamGroup == null)
        {
            return;
        }

        List<StreamGroupVideoStream> existing = FindByCondition(a => a.StreamGroupId == StreamGroupId).ToList();
        foreach (VideoStreamIDRank videoStreamIDRank in videoStreamIDRanks)
        {
            StreamGroupVideoStream? streamGroupVideoStream = existing.FirstOrDefault(a => a.ChildVideoStreamId == videoStreamIDRank.VideoStreamId);
            if (streamGroupVideoStream != null && streamGroupVideoStream.Rank != videoStreamIDRank.Rank)
            {
                streamGroupVideoStream.Rank = videoStreamIDRank.Rank;
                Update(streamGroupVideoStream);
            }
        }
        await repository.SaveAsync().ConfigureAwait(false);
    }

    private async Task UpdateRanks(int StreamGroupId, CancellationToken cancellationToken)
    {
        List<StreamGroupVideoStream> sgVs = await FindByCondition(a => a.StreamGroupId == StreamGroupId).ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        for (int i = 0; i < sgVs.Count; i++)
        {
            sgVs[i].Rank = i;
            Update(sgVs[i]);
        }

        await repository.SaveAsync().ConfigureAwait(false);
    }

    public async Task SetStreamGroupVideoStreamsIsReadOnly(int StreamGroupId, List<string> toUpdate, bool IsReadOnly, CancellationToken cancellationToken)
    {
        await FindAll()
               .Where(a => a.StreamGroupId == StreamGroupId && toUpdate.Contains(a.ChildVideoStreamId))
               .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsReadOnly, IsReadOnly), cancellationToken: cancellationToken)
               .ConfigureAwait(false);
    }

    public async Task<StreamGroupDto?> SyncVideoStreamToStreamGroup(int StreamGroupId, string VideoStreamId, CancellationToken cancellationToken = default)
    {
        // Fetch the stream group by ID
        StreamGroup? targetStreamGroup = await repository.StreamGroup.GetStreamGroupByIdAsync(StreamGroupId).ConfigureAwait(false);

        if (targetStreamGroup == null)
        {
            return null;
        }

        // Check if the stream is already associated with the group
        StreamGroupVideoStream? isStreamAssociated = repository.StreamGroupVideoStream
            .FindByCondition(stream => stream.StreamGroupId == StreamGroupId && stream.ChildVideoStreamId == VideoStreamId)
            .FirstOrDefault();

        if (isStreamAssociated != null)
        {
            if (isStreamAssociated.IsReadOnly)
            {
                return null;
            }
            await RemoveStreamGroupVideoStreams(StreamGroupId, new List<string> { VideoStreamId }, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await AddVideoStreamToStreamGroup(StreamGroupId, VideoStreamId, cancellationToken).ConfigureAwait(false);
        }

        // Fetch and return the updated stream group
        return await sender.Send(new GetStreamGroup(StreamGroupId), cancellationToken);
    }

    public async Task<List<VideoStream>> GetStreamGroupVideoStreamsList(int StreamGroupId, CancellationToken cancellationToken)
    {
        if (StreamGroupId < 2)
        {
            return await repository.VideoStream.GetVideoStreamsNotHidden().ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        IQueryable<VideoStream> childQ = FindAll()
       .Include(a => a.ChildVideoStream)
       .Where(a => a.StreamGroupId == StreamGroupId)
       .Select(a => a.ChildVideoStream)
       .Where(a => !a.IsHidden);

        return await childQ.ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);


    }
}