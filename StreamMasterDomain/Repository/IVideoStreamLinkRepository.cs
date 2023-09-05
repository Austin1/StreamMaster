﻿using MediatR;

using StreamMasterDomain.Dto;
using StreamMasterDomain.Pagination;

using System.Threading;

namespace StreamMasterDomain.Repository
{
    public interface IVideoStreamLinkRepository : IRepositoryBase<VideoStreamLink>
    {
        Task<List<string>> GetVideoStreamVideoStreamIds(string videoStreamId, CancellationToken cancellationToken);

        Task<PagedResponse<ChildVideoStreamDto>> GetVideoStreamVideoStreams(VideoStreamLinkParameters parameters, CancellationToken cancellationToken);

        Task AddVideoStreamTodVideoStream(string ParentVideoStreamId, string ChildVideoStreamId, int? Rank, CancellationToken cancellationToken);

        Task RemoveVideoStreamFromVideoStream(string ParentVideoStreamId, string ChildVideoStreamId, CancellationToken cancellationToken);
    }
}