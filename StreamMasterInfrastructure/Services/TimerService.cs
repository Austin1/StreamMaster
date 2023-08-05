﻿using MediatR;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using StreamMasterApplication.EPGFiles.Commands;
using StreamMasterApplication.M3UFiles.Commands;
using StreamMasterApplication.Settings.Queries;

using StreamMasterDomain.Enums;
using StreamMasterDomain.Repository;

namespace StreamMasterInfrastructure.Services;

public class TimerService : IHostedService, IDisposable
{
    private readonly ILogger<TimerService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IServiceProvider _serviceProvider;
    private readonly object Lock = new();

    private Timer? _timer;
    private bool isActive = false;

    public TimerService(
        IServiceProvider serviceProvider,
        IMemoryCache memoryCache,
        ILogger<TimerService> logger
       )
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        //_logger.LogInformation("Timer Service running.");

        _timer = new Timer(async state => await DoWorkAsync(state, cancellationToken), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        //_logger.LogInformation("Timer Service is stopping.");

        _ = (_timer?.Change(Timeout.Infinite, 0));

        return Task.CompletedTask;
    }

    private async Task DoWorkAsync(object? state, CancellationToken cancellationToken)
    {
        lock (Lock)
        {
            if (isActive)
            {
                return;
            }
            isActive = true;
        }

        using IServiceScope scope = _serviceProvider.CreateScope();
        IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        SystemStatus status = new() { IsSystemReady = _memoryCache.IsSystemReady() };

        if (!status.IsSystemReady)
        {
            lock (Lock)
            {
                isActive = false;
            }
            return;
        }

        IRepositoryWrapper repository = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();

        //_logger.LogInformation("Timer Service is working.");

        DateTime now = DateTime.Now;

        List<EPGFile> epgFiles = new();
        List<M3UFile> m3uFiles = new();

        IEnumerable<EPGFile> epgFilesRepo = await repository.EPGFile.GetAllEPGFilesAsync();
        IEnumerable<EPGFile> epgFilesToUpdated = epgFilesRepo.Where(a => a.AutoUpdate && string.IsNullOrEmpty(a.Url));
        foreach (EPGFile? epgFile in epgFilesToUpdated)
        {
            string epgPath = Path.Combine(FileDefinitions.EPG.DirectoryLocation, epgFile.Source);
            if (!File.Exists(epgPath))
            {
                continue;
            }

            if (File.GetLastWriteTime(epgPath) <= epgFile.LastDownloaded)
            {
                continue;
            }
            epgFiles.Add(epgFile);
        }

        epgFilesRepo = await repository.EPGFile.GetAllEPGFilesAsync();
        epgFilesToUpdated = epgFilesRepo.Where(a => a.AutoUpdate && !string.IsNullOrEmpty(a.Url) && a.HoursToUpdate > 0 && a.LastDownloaded.AddHours(a.HoursToUpdate) < now);
        epgFiles.AddRange(epgFilesToUpdated);

        //   epgFiles.AddRange(context.EPGFiles.Where(a => a.AutoUpdate && !string.IsNullOrEmpty(a.Url) && a.HoursToUpdate > 0 && a.LastDownloaded.AddHours(a.HoursToUpdate) < now));

        IEnumerable<M3UFile> m3uFilesRepo = await repository.M3UFile.GetAllM3UFilesAsync();
        IEnumerable<M3UFile> m3uFilesToUpdated = m3uFilesRepo.Where(a => a.AutoUpdate && string.IsNullOrEmpty(a.Url));
        foreach (M3UFile? m3uFile in m3uFilesToUpdated)
        {
            string m3uPath = Path.Combine(FileDefinitions.M3U.DirectoryLocation, m3uFile.Source);
            if (!File.Exists(m3uPath))
            {
                continue;
            }

            if (File.GetLastWriteTime(m3uPath) <= m3uFile.LastDownloaded)
            {
                continue;
            }
            m3uFiles.Add(m3uFile);
        }

        m3uFilesRepo = await repository.M3UFile.GetAllM3UFilesAsync();
        m3uFilesToUpdated = m3uFilesRepo.Where(a => a.AutoUpdate && !string.IsNullOrEmpty(a.Url) && a.HoursToUpdate > 0 && a.LastDownloaded.AddHours(a.HoursToUpdate) < now);
        m3uFiles.AddRange(m3uFilesToUpdated);

        if (epgFiles.Any())
        {
            _logger.LogInformation("EPG Files to update count: {epgFiles.Count()}", epgFiles.Count());
            foreach (EPGFile epgFile in epgFiles)
            {
                _ = await mediator.Send(new RefreshEPGFileRequest { Id = epgFile.Id }, cancellationToken).ConfigureAwait(false);
            }
        }

        if (m3uFiles.Any())
        {
            _logger.LogInformation("M3U Files to update count: {m3uFiles.Count()}", m3uFiles.Count());
            foreach (M3UFile m3uFile in m3uFiles)
            {
                _ = await mediator.Send(new RefreshM3UFileRequest { Id = m3uFile.Id }, cancellationToken).ConfigureAwait(false);
            }
        }

        lock (Lock)
        {
            isActive = false;
        }
    }
}