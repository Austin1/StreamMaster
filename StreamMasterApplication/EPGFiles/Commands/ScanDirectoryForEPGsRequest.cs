﻿using AutoMapper;

using MediatR;

using Microsoft.Extensions.Logging;

using StreamMasterApplication.M3UFiles.Commands;

using StreamMasterDomain.Dto;

namespace StreamMasterApplication.EPGFiles.Commands;

public record ScanDirectoryForEPGFilesRequest : IRequest<bool> { }

public class ScanDirectoryForEPGFilesRequestHandler : BaseMediatorRequestHandler, IRequestHandler<ScanDirectoryForEPGFilesRequest, bool>
{
    public ScanDirectoryForEPGFilesRequestHandler(ILogger<CreateM3UFileRequestHandler> logger, IRepositoryWrapper repository, IMapper mapper, IPublisher publisher, ISender sender)
        : base(logger, repository, mapper, publisher, sender) { }

    public async Task<bool> Handle(ScanDirectoryForEPGFilesRequest command, CancellationToken cancellationToken)
    {
        IEnumerable<FileInfo> epgFiles = GetEPGFilesFromDirectory();
        foreach (FileInfo epgFileInfo in epgFiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            await ProcessEPGFile(epgFileInfo, cancellationToken);
        }

        return true;
    }

    private IEnumerable<FileInfo> GetEPGFilesFromDirectory()
    {
        FileDefinition fd = FileDefinitions.EPG;
        DirectoryInfo epgDirInfo = new(fd.DirectoryLocation);
        EnumerationOptions er = new() { MatchCasing = MatchCasing.CaseInsensitive };

        return epgDirInfo.GetFiles("*.*", SearchOption.AllDirectories)
            .Where(s => s.FullName.ToLower().EndsWith(fd.FileExtension.ToLower()) ||
                       s.FullName.ToLower().EndsWith(fd.FileExtension + ".gz".ToLower()));
    }

    private async Task ProcessEPGFile(FileInfo epgFileInfo, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(epgFileInfo.DirectoryName))
        {
            return;
        }

        EPGFile? epgFile = await Repository.EPGFile.GetEPGFileBySourceAsync(epgFileInfo.Name).ConfigureAwait(false);
        if (epgFile == null)
        {
            epgFile = CreateOrUpdateEPGFile(epgFileInfo);
            await SaveAndPublishEPGFile(epgFile, cancellationToken);
        }
    }

    private EPGFile CreateOrUpdateEPGFile(FileInfo epgFileInfo)
    {
        EPGFile epgFile = EPGFile.ReadJSON(epgFileInfo) ?? new EPGFile
        {
            Name = Path.GetFileNameWithoutExtension(epgFileInfo.Name),
            Source = epgFileInfo.Name,
            Description = $"Imported from {epgFileInfo.Name}",
            FileExists = true,
            LastDownloaded = epgFileInfo.LastWriteTime,
            LastDownloadAttempt = epgFileInfo.LastWriteTime,
            Url = ""
        };

        if (epgFile.LastUpdated == default)
        {
            epgFile.LastUpdated = DateTime.MinValue;
        }

        epgFile.SetFileDefinition(FileDefinitions.EPG);

        return epgFile;
    }

    private async Task SaveAndPublishEPGFile(EPGFile epgFile, CancellationToken cancellationToken)
    {
        Repository.EPGFile.CreateEPGFile(epgFile);
        _ = await Repository.SaveAsync().ConfigureAwait(false);
        epgFile.WriteJSON();

        if (string.IsNullOrEmpty(epgFile.Url))
        {
            epgFile.LastDownloaded = DateTime.Now;
            Repository.EPGFile.UpdateEPGFile(epgFile);
            _ = await Repository.SaveAsync().ConfigureAwait(false);
            epgFile.WriteJSON();
        }

        EPGFilesDto ret = Mapper.Map<EPGFilesDto>(epgFile);
        await Publisher.Publish(new EPGFileAddedEvent(ret), cancellationToken).ConfigureAwait(false);
    }
}