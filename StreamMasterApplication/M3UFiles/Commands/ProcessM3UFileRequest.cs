﻿using AutoMapper;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using StreamMasterApplication.ChannelGroups.Commands;

using StreamMasterDomain.Extensions;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace StreamMasterApplication.M3UFiles.Commands;

public class ProcessM3UFileRequest : IRequest<M3UFile?>
{
    [Required]
    public int Id { get; set; }
}

public class ProcessM3UFileRequestValidator : AbstractValidator<ProcessM3UFileRequest>
{
    public ProcessM3UFileRequestValidator()
    {
        _ = RuleFor(v => v.Id)
            .NotNull()
            .GreaterThanOrEqualTo(0);
    }
}

public class ProcessM3UFileRequestHandler : BaseMemoryRequestHandler, IRequestHandler<ProcessM3UFileRequest, M3UFile?>
{
    private SimpleIntList existingChannels;

    public ProcessM3UFileRequestHandler(ILogger<ProcessM3UFileRequest> logger, IRepositoryWrapper repository, IMapper mapper, IPublisher publisher, ISender sender, IMemoryCache memoryCache)
        : base(logger, repository, mapper, publisher, sender, memoryCache) { }

    public async Task<M3UFile?> Handle(ProcessM3UFileRequest request, CancellationToken cancellationToken)
    {
        try
        {
            M3UFile? m3uFile = await FetchM3UFile(request.Id).ConfigureAwait(false);
            if (m3uFile == null)
            {
                Logger.LogCritical("Could not find M3U file");
                return null;
            }

            (List<VideoStream>? streams, int streamCount) = await ProcessStreams(m3uFile).ConfigureAwait(false);
            if (streams == null)
            {
                Logger.LogCritical("Error while processing M3U file, bad format");
                return null;
            }

            if (streams.Count == 0)
            {
                return m3uFile;
            }

            if (!ShouldUpdate(m3uFile))
            {
                return m3uFile;
            }

            await ProcessAndUpdateStreams(m3uFile, streams, streamCount).ConfigureAwait(false);
            await UpdateChannelGroups(streams, cancellationToken).ConfigureAwait(false);
            await NotifyUpdates(m3uFile, cancellationToken).ConfigureAwait(false);

            return m3uFile;
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error while processing M3U file");
            return null;
        }
    }

    private async Task<M3UFile?> FetchM3UFile(int id)
    {
        return await Repository.M3UFile.GetM3UFileByIdAsync(id).ConfigureAwait(false);
    }

    private async Task<(List<VideoStream>? streams, int streamCount)> ProcessStreams(M3UFile m3uFile)
    {
        Stopwatch sw = Stopwatch.StartNew();

        List<VideoStream>? streams = await m3uFile.GetM3U().ConfigureAwait(false);

        int streamsCount = 0;
        if (streams != null)
        {
            streamsCount = GetRealStreamCount(streams);
            streams = RemoveIgnoredStreams(streams);
            streams = RemoveDuplicates(streams);
        }

        Logger.LogInformation($"Processing M3U streams took {sw.Elapsed.TotalSeconds} seconds");
        return (streams, streamsCount);
    }

    private bool ShouldUpdate(M3UFile m3uFile)
    {
        return m3uFile.LastWrite() >= m3uFile.LastUpdated;
    }

    private async Task ProcessAndUpdateStreams(M3UFile m3uFile, List<VideoStream> streams, int streamCount)
    {
        Stopwatch sw = Stopwatch.StartNew();

        List<VideoStream> existing = await Repository.VideoStream.GetVideoStreamsByM3UFileId(m3uFile.Id).ToListAsync();
        existingChannels = new SimpleIntList(m3uFile.StartingChannelNumber < 0 ? 0 : m3uFile.StartingChannelNumber - 1);

        List<ChannelGroup> groups = Repository.ChannelGroup.GetAllChannelGroups().ToList();
        //var nextchno = m3uFile.StartingChannelNumber < 0 ? 0 : m3uFile.StartingChannelNumber;

        //List<VideoStream> toWrite = new();
        //List<VideoStream> toUpdate = new();

        ProcessStreamsConcurrently(streams, existing, groups, m3uFile);

        //if (toWrite.Any())
        //{
        //    Repository.VideoStream.BulkInsert(toWrite.ToArray());
        //}

        //if (toUpdate.Any())
        //{
        //    Repository.VideoStream.BulkUpdate(toUpdate.ToArray());
        //}

        int testcount = await Repository.SaveAsync().ConfigureAwait(false);
        Logger.LogInformation($"testcount {testcount}");
        m3uFile.LastUpdated = DateTime.Now;
        if (m3uFile.StationCount != streamCount)
        {
            m3uFile.StationCount = streamCount;
        }
        Repository.M3UFile.UpdateM3UFile(m3uFile);
        int testcount2 = await Repository.SaveAsync().ConfigureAwait(false);
        Logger.LogInformation($"testcount2 {testcount2}");
        Logger.LogInformation($"Processing and updating streams took {sw.Elapsed.TotalSeconds} seconds");
    }

    private void ProcessStreamsConcurrently(List<VideoStream> streams, List<VideoStream> existing, List<ChannelGroup> groups, M3UFile m3uFile)
    {
        int totalCount = streams.Count;
        int processedCount = 0;
        Dictionary<string, VideoStream> existingLookup = existing.ToDictionary(a => a.Id, a => a);
        Dictionary<string, ChannelGroup> groupLookup = groups.ToDictionary(g => g.Name, g => g);

        List<VideoStream> toWrite = new();
        List<VideoStream> toUpdate = new();

        foreach (VideoStream stream in streams)
        {
            _ = groupLookup.TryGetValue(stream.Tvg_group, out ChannelGroup? group);

            if (!existingLookup.ContainsKey(stream.Id))
            {
                ProcessNewStream(stream, group?.IsHidden ?? false, m3uFile.Name);
                toWrite.Add(stream);
            }
            else
            {
                VideoStream? dbStream = existingLookup[stream.Id];
                if (ProcessExistingStream(stream, dbStream, group?.IsHidden ?? false, m3uFile.Name))
                {
                    toUpdate.Add(dbStream);
                }
            }

            processedCount++;
            if (processedCount % 10000 == 0)
            {
                // Logging
                Logger.LogInformation($"Processed {processedCount}/{totalCount} streams, adding {toWrite.Count}, updating: {toUpdate.Count}");

                Repository.VideoStream.BulkInsert(toWrite.ToArray());
                toWrite.Clear();

                Repository.VideoStream.BulkUpdate(toUpdate.ToArray());
                toUpdate.Clear();
            }
        }

        // Handle any remaining items after the loop
        if (toWrite.Any())
        {
            Repository.VideoStream.BulkInsert(toWrite.ToArray());
        }
        if (toUpdate.Any())
        {
            Repository.VideoStream.BulkUpdate(toUpdate.ToArray());
        }
    }


    private async Task UpdateChannelGroups(List<VideoStream> streams, CancellationToken cancellationToken)
    {
        Stopwatch sw = Stopwatch.StartNew();

        List<string> newGroups = streams.Where(a => a.User_Tvg_group != null).Select(a => a.User_Tvg_group).Distinct().ToList();
        List<ChannelGroup> channelGroups = await Repository.ChannelGroup.GetAllChannelGroups().ToListAsync(cancellationToken);

        await CreateNewChannelGroups(newGroups, channelGroups, cancellationToken);

        Logger.LogInformation($"Updating channel groups took {sw.Elapsed.TotalSeconds} seconds");
    }

    private async Task CreateNewChannelGroups(List<string> newGroups, List<ChannelGroup> existingGroups, CancellationToken cancellationToken)
    {
        int rank = existingGroups.Any() ? existingGroups.Max(a => a.Rank) + 1 : 1;

        foreach (string? group in newGroups)
        {
            if (!existingGroups.Any(a => a.Name == group))
            {
                await Sender.Send(new CreateChannelGroupRequest(group, rank++, true), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task NotifyUpdates(M3UFile m3uFile, CancellationToken cancellationToken)
    {
        List<string> m3uChannelGroupNames = await Repository.M3UFile.GetChannelGroupNamesFromM3UFile(m3uFile.Id);
        List<ChannelGroup> channelGroups = await Repository.ChannelGroup.GetChannelGroupsFromNames(m3uChannelGroupNames);

        await Sender.Send(new UpdateChannelGroupCountsRequest(channelGroups.Select(a => a.Id)), cancellationToken).ConfigureAwait(false);
        await Publisher.Publish(new M3UFileProcessedEvent(), cancellationToken).ConfigureAwait(false);
    }

    private List<VideoStream> RemoveIgnoredStreams(List<VideoStream> streams)
    {

        if (Settings.NameRegex.Any())
        {
            foreach (string regex in Settings.NameRegex)
            {
                List<VideoStream> toIgnore = ListHelper.GetMatchingProperty(streams, "Tvg_name", regex);
                _ = streams.RemoveAll(toIgnore.Contains);
            }
        }

        return streams;
    }

    private int GetRealStreamCount(List<VideoStream> streams)
    {
        List<string> ids = streams.Select(a => a.Id).Distinct().ToList();
        return ids.Count;
    }

    private List<VideoStream> RemoveDuplicates(List<VideoStream> streams)
    {

        List<VideoStream> cleanStreams = streams.GroupBy(s => s.Id)
                  .Select(g => g.First())
                  .ToList();

        //List<VideoStream> dupes = Repository.VideoStream.FindByCondition(a => ids.Contains(a.Id)).ToList();

        //List<VideoStream> duplicateIds = streams.GroupBy(x => x.Id).Where(g => g.Count() > 1).First().ToList();

        //if (ids.Any())
        //{
        //    List<string> dupeIds = dupes.Select(a => a.Id).Distinct().ToList();

        //    //LogDuplicatesToCSV(dupes);
        //    streams = streams.Where(a => !dupeIds.Contains(a.Id)).ToList();
        //}

        return cleanStreams;
    }

    private void LogDuplicatesToCSV(List<VideoStream> dupes)
    {
        string fileName = $"dupes.csv";
        List<string> lines = new() { VideoStream.GetCsvHeader() };
        lines.AddRange(dupes.Select(a => a.ToString()));

        using StreamWriter file = new(fileName);
        foreach (string line in lines)
        {
            file.WriteLine(line);
        }

        Logger.LogError($"Found duplicate streams. Details logged to {fileName}");
    }

    private bool ProcessExistingStream(VideoStream stream, VideoStream dbStream, bool isHidden, string mu3FileName)
    {
        bool changed = false;
        //if (dbStream.IsHidden != isHidden)
        //{
        //    changed = true;
        //    dbStream.IsHidden = isHidden;
        //}

        if (dbStream.Tvg_group != stream.Tvg_group)
        {
            changed = true;
            dbStream.Tvg_group = stream.Tvg_group;
        }

        if (string.IsNullOrEmpty(dbStream.M3UFileName) || dbStream.M3UFileName != mu3FileName)
        {
            changed = true;
            dbStream.M3UFileName = mu3FileName;
        }

        if (Settings.OverWriteM3UChannels || dbStream.Tvg_chno != stream.Tvg_chno)
        {
            int localNextChno = Settings.OverWriteM3UChannels ? existingChannels.GetNextInt() : existingChannels.GetNextInt(stream.Tvg_chno);
            if (dbStream.Tvg_chno != localNextChno)
            {
                changed = true;
                dbStream.Tvg_chno = localNextChno;
            }
        }

        if (dbStream.Tvg_ID != stream.Tvg_ID)
        {
            changed = true;
            dbStream.Tvg_ID = stream.Tvg_ID;
        }

        if (dbStream.Tvg_logo != stream.Tvg_logo)
        {
            changed = true;

            dbStream.Tvg_logo = stream.Tvg_logo;
        }

        if (dbStream.Tvg_name != stream.Tvg_name)
        {
            changed = true;

            dbStream.Tvg_name = stream.Tvg_name;
        }

        return changed;
    }

    private bool ProcessExistingUserStream(VideoStream stream, VideoStream dbStream, bool isHidden, string mu3FileName)
    {
        bool changed = false;
        //if (dbStream.IsHidden != isHidden)
        //{
        //    changed = true;
        //    dbStream.IsHidden = isHidden;
        //}

        if (dbStream.User_Tvg_group != stream.Tvg_group)
        {
            changed = true;
            dbStream.User_Tvg_group = stream.Tvg_group;
        }

        if (string.IsNullOrEmpty(dbStream.M3UFileName) || dbStream.M3UFileName != mu3FileName)
        {
            changed = true;
            dbStream.M3UFileName = mu3FileName;
        }

        if (Settings.OverWriteM3UChannels || dbStream.User_Tvg_chno != stream.Tvg_chno)
        {
            int localNextChno = Settings.OverWriteM3UChannels ? existingChannels.GetNextInt() : existingChannels.GetNextInt(stream.Tvg_chno);
            if (dbStream.User_Tvg_chno != localNextChno)
            {
                changed = true;
                dbStream.User_Tvg_chno = localNextChno;
            }
        }

        if (dbStream.User_Tvg_ID != stream.Tvg_ID)
        {
            changed = true;
            dbStream.User_Tvg_ID = stream.Tvg_ID;
        }

        if (dbStream.User_Tvg_logo != stream.Tvg_logo)
        {
            changed = true;

            dbStream.User_Tvg_logo = stream.Tvg_logo;
        }

        if (dbStream.User_Tvg_name != stream.Tvg_name)
        {
            changed = true;

            dbStream.User_Tvg_name = stream.Tvg_name;
        }

        return changed;
    }


    private void ProcessNewStream(VideoStream stream, bool IsHidden, string mu3FileName)
    {
        stream.IsHidden = IsHidden;

        if (Settings.OverWriteM3UChannels || stream.User_Tvg_chno == 0 || existingChannels.ContainsInt(stream.Tvg_chno))
        {
            int localNextChno = Settings.OverWriteM3UChannels ? existingChannels.GetNextInt() : existingChannels.GetNextInt(stream.User_Tvg_chno);

            stream.User_Tvg_chno = localNextChno;
            stream.Tvg_chno = localNextChno;
        }
        stream.M3UFileName = mu3FileName;
    }

}