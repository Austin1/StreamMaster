﻿using StreamMasterDomain.Dto;

namespace StreamMasterApplication.Common.Models
{
    /// <summary>
    /// Defines the configuration options for a video streamer.
    /// </summary>
    public class StreamerConfiguration
    {
        public StreamerConfiguration()
        {
            BufferSize = 1 * 1024 * 1000;
            BufferChunkSize = 4096;
            FailoverCheckInterval = 200;
            MaxConnectRetries = 20;
            MaxConnectRetryTime = 100;
            PreloadPercentage = .5;
            BroadcastInterval = 3;
        }

        public int BroadcastInterval { get; set; }

        /// <summary>
        /// Gets or sets the size of each chunk of data read from the video stream.
        /// </summary>
        public int BufferChunkSize { get; set; }

        /// <summary>
        /// Gets or sets the size of the buffer used to store the video stream data.
        /// </summary>
        public int BufferSize { get; set; }

        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the client.
        /// </summary>
        public Guid ClientId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the IPTV channel information.
        /// </summary>
        public VideoStreamDto? CurentVideoStream { get; set; }

        public int CurrentM3UFileId { get; set; }

        /// <summary>
        /// Gets or sets the interval (in milliseconds) at which the failover
        /// process checks for a new stream URL.
        /// </summary>
        public int FailoverCheckInterval { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retries allowed when attempting
        /// to connect to the video stream.
        /// </summary>
        public int MaxConnectRetries { get; set; }

        ///// <summary>
        ///// Gets or sets the identifier for the M3U stream.
        ///// </summary>
        //public int ExtendedVideoStreamDtoId { get; set; }
        public int MaxConnectRetryTime { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the buffer that must be preloaded
        /// before clients are allowed to read from it.
        /// </summary>
        public double PreloadPercentage { get; set; }

        ///// <summary>
        ///// Gets or sets the IPTV channel information.
        ///// </summary>
        //public VideoStreamDto? VideoStream { get; set; }

        public VideoStreamHandlers VideoStreamHandler { get; set; } = VideoStreamHandlers.SystemDefault;
        public List<VideoStreamDto>? VideoStreams { get; set; }
    }
}