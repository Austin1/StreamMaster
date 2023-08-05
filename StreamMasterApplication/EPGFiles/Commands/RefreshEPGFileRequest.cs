﻿using AutoMapper;

using FluentValidation;

using MediatR;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using StreamMasterDomain.Attributes;
using StreamMasterDomain.Dto;
using StreamMasterDomain.Repository;
using StreamMasterDomain.Repository.EPG;

namespace StreamMasterApplication.EPGFiles.Commands;

[RequireAll]
public class RefreshEPGFileRequest : IRequest<EPGFilesDto?>
{
    //public bool ForceDownload { get; set; }
    public int EPGFileID { get; set; }
}

public class RefreshEPGFileRequestValidator : AbstractValidator<RefreshEPGFileRequest>
{
    public RefreshEPGFileRequestValidator()
    {
        _ = RuleFor(v => v.EPGFileID).NotNull().GreaterThanOrEqualTo(0);
    }
}

public class RefreshEPGFileRequestHandler : IRequestHandler<RefreshEPGFileRequest, EPGFilesDto?>
{
    private readonly IAppDbContext _context;
    private readonly ILogger<RefreshEPGFileRequest> _logger;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _memoryCache;
    private readonly IPublisher _publisher;

    public RefreshEPGFileRequestHandler(
        ILogger<RefreshEPGFileRequest> logger,
        IMapper mapper,
        IMemoryCache memoryCache,
        IPublisher publisher,

        IAppDbContext context)
    {
        _publisher = publisher;
        _mapper = mapper;
        _logger = logger;
        _memoryCache = memoryCache;
        _context = context;
    }

    public async Task<EPGFilesDto?> Handle(RefreshEPGFileRequest request, CancellationToken cancellationToken)
    {
        try
        {
            EPGFile? epgFile = await _context.EPGFiles.FindAsync(new object?[] { request.EPGFileID, cancellationToken }, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (epgFile == null)
            {
                return null;
            }

            if (epgFile.LastDownloadAttempt.AddMinutes(epgFile.MinimumMinutesBetweenDownloads) < DateTime.Now)
            {
                FileDefinition fd = FileDefinitions.EPG;
                string fullName = Path.Combine(fd.DirectoryLocation, epgFile.Source);

                if (epgFile.Url != null && epgFile.Url.Contains("://"))
                {
                    _logger.LogInformation("Refresh EPG From URL {epgFile.Url}", epgFile.Url);

                    epgFile.LastDownloadAttempt = DateTime.Now;

                    (bool success, Exception? ex) = await FileUtil.DownloadUrlAsync(epgFile.Url, fullName, cancellationToken).ConfigureAwait(false);
                    if (success)
                    {
                        epgFile.DownloadErrors = 0;
                        epgFile.LastDownloaded = File.GetLastWriteTime(fullName);
                        epgFile.FileExists = true;
                    }
                    else
                    {
                        ++epgFile.DownloadErrors;
                        _logger.LogCritical("Exception EPG From URL {ex}", ex);
                    }
                }

                Tv? tv = await epgFile.GetTV().ConfigureAwait(false);
                if (tv == null)
                {
                    _logger.LogCritical("Exception EPG {fullName} format is not supported", fullName);
                    //Bad EPG
                    if (File.Exists(fullName))
                    {
                        File.Delete(fullName);
                    }
                    return null;
                }

                if (tv != null)
                {
                    epgFile.ChannelCount = tv.Channel != null ? tv.Channel.Count : 0;
                    epgFile.ProgrammeCount = tv.Programme != null ? tv.Programme.Count : 0;
                }

                _ = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var programmes = _memoryCache.ChannelLogos();
                programmes.RemoveAll(a => a.EPGFileId == epgFile.Id);
                _memoryCache.Set(programmes);

                var channels = _memoryCache.ChannelLogos();
                channels.RemoveAll(a => a.EPGFileId == epgFile.Id);
                _memoryCache.Set(channels);

                var channelLogos = _memoryCache.ChannelLogos();
                channelLogos.RemoveAll(a => a.EPGFileId == epgFile.Id);
                _memoryCache.Set(channelLogos);

                var programmeIcons = _memoryCache.ProgrammeIcons();
                programmeIcons.RemoveAll(a => a.FileId == epgFile.Id);
                _memoryCache.SetProgrammeLogos(programmeIcons);


                EPGFilesDto ret = _mapper.Map<EPGFilesDto>(epgFile);
                await _publisher.Publish(new EPGFileAddedEvent(ret), cancellationToken).ConfigureAwait(false);
                return ret;
            }
        }
        catch (Exception)
        {
            return null;
        }
        return null;
    }
}
