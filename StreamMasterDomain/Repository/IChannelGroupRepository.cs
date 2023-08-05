﻿using StreamMasterDomain.Dto;

namespace StreamMasterDomain.Repository
{
    public interface IChannelGroupRepository
    {
        IQueryable<ChannelGroup> GetAllChannelGroups();
        Task<ChannelGroup?> GetChannelGroupByNameAsync(string name);
        Task<ChannelGroup?> GetChannelGroupAsync(int Id);
        //Task<IEnumerable<ChannelGroup>> GetChannelGroupsAsync();
        void CreateChannelGroup(ChannelGroup ChannelGroup);
        void DeleteChannelGroup(ChannelGroup ChannelGroup);
        void UpdateChannelGroup(ChannelGroup ChannelGroup);
        Task<(ChannelGroupDto? channelGroup, List<VideoStreamDto>? distinctList, List<StreamGroupDto>? streamGroupIds)> UpdateChannelGroup(UpdateChannelGroupRequest request, string url, CancellationToken cancellationToken);
    }
}