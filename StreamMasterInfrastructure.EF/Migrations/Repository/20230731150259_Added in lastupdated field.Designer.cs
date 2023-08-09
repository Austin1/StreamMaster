﻿// <auto-generated />
using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using StreamMasterInfrastructureEF;

#nullable disable

namespace StreamMasterInfrastructure.Persistence.Migrations
{
    [DbContext(typeof(RepositoryContext))]
    [Migration("20230731150259_Added in lastupdated field")]
    partial class Addedinlastupdatedfield
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.9");

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FriendlyName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Xml")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("DataProtectionKeys");
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.ChannelGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsReadOnly")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Rank")
                        .HasColumnType("INTEGER");

                    b.Property<string>("RegexMatch")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ChannelGroups");
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.EPGFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AutoUpdate")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChannelCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("DownloadErrors")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EPGRank")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FileExists")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileExtension")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("HoursToUpdate")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastDownloadAttempt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastDownloaded")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("TEXT");

                    b.Property<int>("MinimumMinutesBetweenDownloads")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ProgrammeCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SMFileType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<float>("TimeShift")
                        .HasColumnType("REAL");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("EPGFiles", (string)null);
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.M3UFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AutoUpdate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("DownloadErrors")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("FileExists")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileExtension")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("HoursToUpdate")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastDownloadAttempt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastDownloaded")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("TEXT");

                    b.Property<int>("MaxStreamCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MinimumMinutesBetweenDownloads")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("SMFileType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("StartingChannelNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StationCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("M3UFiles", (string)null);
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.StreamGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("StreamGroupNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("StreamGroups");
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.StreamGroupChannelGroup", b =>
                {
                    b.Property<int>("ChannelGroupId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StreamGroupId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ChannelGroupId", "StreamGroupId");

                    b.HasIndex("StreamGroupId");

                    b.ToTable("StreamGroupChannelGroups");
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.StreamGroupVideoStream", b =>
                {
                    b.Property<string>("ChildVideoStreamId")
                        .HasColumnType("TEXT");

                    b.Property<int>("StreamGroupId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsReadOnly")
                        .HasColumnType("INTEGER");

                    b.HasKey("ChildVideoStreamId", "StreamGroupId");

                    b.HasIndex("StreamGroupId");

                    b.ToTable("StreamGroupVideoStreams");
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.VideoStream", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("FilePosition")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsReadOnly")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsUserCreated")
                        .HasColumnType("INTEGER");

                    b.Property<int>("M3UFileId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StreamProxyType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Tvg_ID")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Tvg_chno")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Tvg_group")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Tvg_logo")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Tvg_name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("User_Tvg_ID")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("User_Tvg_chno")
                        .HasColumnType("INTEGER");

                    b.Property<string>("User_Tvg_group")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("User_Tvg_logo")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("User_Tvg_name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("User_Url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("VideoStreamHandler")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("VideoStreams");
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.VideoStreamLink", b =>
                {
                    b.Property<string>("ParentVideoStreamId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ChildVideoStreamId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Rank")
                        .HasColumnType("INTEGER");

                    b.HasKey("ParentVideoStreamId", "ChildVideoStreamId");

                    b.HasIndex("ChildVideoStreamId");

                    b.ToTable("VideoStreamLinks");
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.StreamGroupChannelGroup", b =>
                {
                    b.HasOne("StreamMasterDomain.Entities.ChannelGroup", "ChannelGroup")
                        .WithMany()
                        .HasForeignKey("ChannelGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StreamMasterDomain.Entities.StreamGroup", "StreamGroup")
                        .WithMany("ChannelGroups")
                        .HasForeignKey("StreamGroupId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ChannelGroup");

                    b.Navigation("StreamGroup");
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.StreamGroupVideoStream", b =>
                {
                    b.HasOne("StreamMasterDomain.Entities.VideoStream", "ChildVideoStream")
                        .WithMany("StreamGroups")
                        .HasForeignKey("ChildVideoStreamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("StreamMasterDomain.Entities.StreamGroup", null)
                        .WithMany("ChildVideoStreams")
                        .HasForeignKey("StreamGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ChildVideoStream");
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.VideoStreamLink", b =>
                {
                    b.HasOne("StreamMasterDomain.Entities.VideoStream", "ChildVideoStream")
                        .WithMany()
                        .HasForeignKey("ChildVideoStreamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StreamMasterDomain.Entities.VideoStream", "ParentVideoStream")
                        .WithMany("ChildVideoStreams")
                        .HasForeignKey("ParentVideoStreamId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ChildVideoStream");

                    b.Navigation("ParentVideoStream");
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.StreamGroup", b =>
                {
                    b.Navigation("ChannelGroups");

                    b.Navigation("ChildVideoStreams");
                });

            modelBuilder.Entity("StreamMasterDomain.Entities.VideoStream", b =>
                {
                    b.Navigation("ChildVideoStreams");

                    b.Navigation("StreamGroups");
                });
#pragma warning restore 612, 618
        }
    }
}
