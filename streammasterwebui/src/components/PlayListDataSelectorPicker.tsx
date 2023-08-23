
import { Button } from "primereact/button";
import { type VideoStreamsGetVideoStreamsApiArg, type StreamGroupsGetStreamGroupsApiArg, type VideoStreamDto, type ChildVideoStreamDto, type StreamGroupDto, type UpdateVideoStreamRequest, type UpdateStreamGroupRequest, type VideoStreamIsReadOnly, useStreamGroupsUpdateStreamGroupMutation } from "../store/iptvApi";
import { useVideoStreamsGetVideoStreamsQuery, useStreamGroupsGetStreamGroupsQuery, useVideoStreamsUpdateVideoStreamMutation } from "../store/iptvApi";
import { useEPGColumnConfig } from "./columns/columnConfigHooks";
import { Toast } from "primereact/toast";
import { type CSSProperties, type ReactNode } from "react";
import { useRef, useState, useEffect, useCallback, memo } from "react";

import { getTopToolOptions } from "../common/common";
import { GroupIcon } from "../common/icons";
import DataSelectorPicker from "../features/dataSelectorPicker/DataSelectorPicker";
import ChannelNameEditor from "./ChannelNameEditor";
import ChannelNumberEditor from "./ChannelNumberEditor";
import { type ColumnMeta } from "./dataSelector/DataSelectorTypes";
import IconSelector from "./selectors/IconSelector";
import { Tooltip } from "primereact/tooltip";

const PlayListDataSelectorPicker = (props: PlayListDataSelectorPickerProps) => {
  const toast = useRef<Toast>(null);

  const videoStreamsQuery = useVideoStreamsGetVideoStreamsQuery({} as VideoStreamsGetVideoStreamsApiArg);
  const streamGroupsQuery = useStreamGroupsGetStreamGroupsQuery({} as StreamGroupsGetStreamGroupsApiArg);
  const [videoStreamsUpdateVideoStreamMutation] = useVideoStreamsUpdateVideoStreamMutation();

  const [sourceVideoStreams, setSourceVideoStreams] = useState<VideoStreamDto[] | undefined>(undefined);
  const [targetVideoStreams, setTargetVideoStreams] = useState<ChildVideoStreamDto[] | undefined>(undefined);
  const [isVideoStreamUpdating, setIsVideoStreamUpdating] = useState<boolean>(false);
  const [streamGroup, setStreamGroup] = useState<StreamGroupDto | undefined>(undefined);
  const { columnConfig: epgColumnConfig } = useEPGColumnConfig(true);
  const [streamGroupsUpdateStreamGroupMutation] = useStreamGroupsUpdateStreamGroupMutation();

  useEffect(() => {
    if (!props.streamGroup || props.streamGroup === undefined || streamGroupsQuery.data === undefined || streamGroupsQuery.data.data === undefined) {
      setStreamGroup(undefined);
      return;
    }

    const sg = streamGroupsQuery.data.data.find((x: StreamGroupDto) => x.id === props.streamGroup?.id);
    if (sg === null || sg === undefined) {
      setStreamGroup({} as StreamGroupDto);
      return;
    }

    setStreamGroup(sg);
    return () => setStreamGroup(undefined);

  }, [props.streamGroup, streamGroupsQuery.data])

  useEffect(() => {
    if (!videoStreamsQuery.data) {
      return;
    }

    if (props.videoStream?.childVideoStreams !== undefined && props.videoStream.childVideoStreams.length > 0) {
      const newStream = videoStreamsQuery.data.data.find((m3u: VideoStreamDto) => m3u.id === props.videoStream?.id);
      if (newStream === undefined || newStream.childVideoStreams === undefined) {
        return;
      }

      const newStreams = [...newStream.childVideoStreams];
      const dsIds = newStreams.map((sgvs) => sgvs.id);

      const toSet = newStreams.sort((a, b) => a.rank - b.rank);


      setTargetVideoStreams(toSet);

      if (props.showTriState === null) {
        setSourceVideoStreams(videoStreamsQuery.data.data.filter((m3u) => !dsIds?.includes(m3u.id)));
      } else {
        setSourceVideoStreams(videoStreamsQuery.data.data.filter((m3u) => m3u.isHidden !== props.showTriState && !dsIds?.includes(m3u.id)));
      }

      return;
    }


    if (streamGroup === undefined || streamGroup.id === undefined || streamGroup.childVideoStreams === undefined) {
      const newData = [...videoStreamsQuery.data.data];

      if (props.showTriState === null) {
        setSourceVideoStreams(newData.sort((a, b) => a.user_Tvg_name.localeCompare(b.user_Tvg_name)));
      } else {
        setSourceVideoStreams(newData.filter((m3u) => m3u.isHidden !== props.showTriState).sort((a, b) => a.user_Tvg_name.localeCompare(b.user_Tvg_name)));
      }

      setTargetVideoStreams(undefined);
      return;
    }


    const ids = streamGroup.childVideoStreams.map((sgvs) => sgvs.id);
    const streams = videoStreamsQuery.data.data.filter((m3u) => ids?.includes(m3u.id));

    const roIds = streamGroup.childVideoStreams.filter((vs) => vs.isReadOnly === true).map((sgvs) => sgvs.id);

    const updatedStreams = streams.map((newStream) => {
      if (roIds.includes(newStream.id)) {
        return { ...newStream, isReadOnly: true };
      }

      return newStream;
    });

    if (props.isAdditionalChannels === true) {
      const toSet = (updatedStreams as ChildVideoStreamDto[]).filter((m3u) => props.showHidden === true || m3u.isHidden !== true).sort((a, b) => a.rank - b.rank);
      setTargetVideoStreams(toSet);
    } else {

      setTargetVideoStreams(
        (updatedStreams as ChildVideoStreamDto[]).filter((m3u) => props.showHidden === true || m3u.isHidden !== true)
      );
    }

    if (props.showTriState === null) {
      setSourceVideoStreams(videoStreamsQuery.data.data.filter((m3u) => !ids?.includes(m3u.id)));
    } else {
      setSourceVideoStreams(videoStreamsQuery.data.data.filter((m3u) => m3u.isHidden !== props.showTriState && !ids?.includes(m3u.id)));
    }

    return () => {
      setSourceVideoStreams(undefined);
      setTargetVideoStreams(undefined);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [props.isAdditionalChannels, props.showHidden, props.showTriState, props.videoStream, streamGroup, videoStreamsQuery.data]);

  const channelNumberEditorBodyTemplate = useCallback((data: VideoStreamDto) => {

    return (
      <ChannelNumberEditor
        data={data}
      />
    )
  }, []);


  const sourceColumns: ColumnMeta[] = [

    {
      bodyTemplate: channelNumberEditorBodyTemplate,
      field: 'user_Tvg_chno',
      filter: false,
      header: 'Ch.',
      sortable: true,
      style: {
        maxWidth: '3rem',
        width: '3rem',
      } as CSSProperties,
    },
    {
      field: 'user_Tvg_name', header: 'Name', sortable: true,
      style: {
        flexGrow: 1,
        flexShrink: 1
      }
    }
    ,
    {
      field: 'user_Tvg_group', header: 'Group', sortable: true,
    }
    ,
    {
      field: 'm3UFileId',
      header: 'File',
      sortable: true,
      style: {
        maxWidth: '3rem',
        width: '3rem',
      } as CSSProperties,
    }
  ];



  const onUpdateVideoStream = useCallback(async (data: VideoStreamDto, Logo: string) => {
    if (data.id === '') {
      return;
    }

    const toSend = {} as UpdateVideoStreamRequest;

    toSend.id = data.id;

    if (Logo && Logo !== '' && data.user_Tvg_logo !== Logo) {
      toSend.tvg_logo = Logo;
    }

    await videoStreamsUpdateVideoStreamMutation(toSend)
      .then(() => {
        if (toast.current) {

          toast.current.show({
            detail: `Updated Stream`,
            life: 3000,
            severity: 'success',
            summary: 'Successful',
          });

        }
      }).catch((e) => {
        if (toast.current) {
          toast.current.show({
            detail: `Update Stream Failed`,
            life: 3000,
            severity: 'error',
            summary: 'Error ' + e.message,
          });
        }
      });

  }, [videoStreamsUpdateVideoStreamMutation]);

  const channelNameEditorBodyTemplate = useCallback((data: VideoStreamDto) => {
    return (
      <ChannelNameEditor
        data={data}

      />
    )
  }, []);

  const logoEditorBodyTemplate = useCallback((data: VideoStreamDto) => {
    return (
      <IconSelector
        className="p-inputtext-sm"
        enableEditMode
        onChange={
          async (e: string) => {
            await onUpdateVideoStream(data, e);
          }
        }
        value={data.user_Tvg_logo}
      />
    );

  }, [onUpdateVideoStream]);

  const onEdit = useCallback(async (data: VideoStreamDto[]) => {
    if (data === null || data === undefined || !props.videoStream) {

      return;
    }

    const toSend = {} as UpdateVideoStreamRequest;

    toSend.id = props.videoStream.id;

    const newData = data.map((x: VideoStreamDto, index: number) => { return { ...x, rank: index, } }) as ChildVideoStreamDto[];

    toSend.childVideoStreams = newData;

    videoStreamsUpdateVideoStreamMutation(toSend)
      .then(() => {
        if (toast.current) {

          toast.current.show({
            detail: `Video Stream Update Successful`,
            life: 3000,
            severity: 'success',
            summary: 'Successful',
          });


        }
      }
      ).catch(() => {
        if (toast.current) {
          toast.current.show({
            detail: `Channel Update Failed`,
            life: 3000,
            severity: 'error',
            summary: 'Error',
          });
        }
      });

  }, [props.videoStream, videoStreamsUpdateVideoStreamMutation]);


  const onSave = useCallback(async (data: VideoStreamDto[]) => {

    if (props.isAdditionalChannels == true) {
      await onEdit(data);
      return;
    }

    if (isVideoStreamUpdating || data.length === 0 || data[0].id === undefined) {
      return;
    }

    setIsVideoStreamUpdating(true);

    var toSend = {} as UpdateStreamGroupRequest;

    if (props.streamGroup)
      toSend.streamGroupId = props.streamGroup.id;

    toSend.videoStreams = data.map((stream) => {
      return { isReadOnly: stream.isReadOnly, videoStreamId: stream.id } as VideoStreamIsReadOnly;
    });

    await streamGroupsUpdateStreamGroupMutation(toSend)
      .then(() => {
        if (toast.current) {

          toast.current.show({
            detail: `Stream Group Update Successful`,
            life: 3000,
            severity: 'success',
            summary: 'Successful',
          });
        }
      }).catch(() => {
        if (toast.current) {
          toast.current.show({
            detail: `Stream Group Update Failed`,
            life: 3000,
            severity: 'error',
            summary: 'Error',
          });
        }
      });

    setIsVideoStreamUpdating(false);
  }, [isVideoStreamUpdating, onEdit, props.isAdditionalChannels, props.streamGroup, streamGroupsUpdateStreamGroupMutation]);

  const onChange = useCallback(async (e: ChildVideoStreamDto[]) => {

    await onSave(e);
  }, [onSave]);

  const onRemoveRank = useCallback(async (data: VideoStreamDto) => {
    if (targetVideoStreams === undefined) {
      return;
    }

    const newtargetVideoStreams = targetVideoStreams.filter((m3u) => m3u.id !== data.id);
    if (props.isAdditionalChannels === true) {
      setTargetVideoStreams(newtargetVideoStreams.sort((a, b) => a.rank - b.rank));
    } else {
      setTargetVideoStreams(newtargetVideoStreams);
    }

    await onSave(newtargetVideoStreams);

  }, [onSave, props, targetVideoStreams]);

  const sourceActionBodyTemplate = useCallback((data: VideoStreamDto) => {

    if (data.isReadOnly === true) {
      return (
        <div className='flex min-w-full min-h-full justify-content-end align-items-center'>
          <Tooltip target=".GroupIcon-class" />
          <div
            className="GroupIcon-class border-white"
            data-pr-at="right+5 top"

            data-pr-hidedelay={100}
            data-pr-my="left center-2"

            data-pr-position="left"
            data-pr-showdelay={500}
            // data-pr-tooltip={`Group: ${data.user_Tvg_group}`}
            data-pr-tooltip='From Group'
          // style={{ minWidth: '10rem' }}
          >
            <GroupIcon />
          </div>
        </div>
      );
    }

    return (
      <Button
        className="p-button-danger"
        icon="pi pi-times"
        onClick={async () => await onRemoveRank(data)}
        rounded
        text
        tooltip="Remove"
        tooltipOptions={getTopToolOptions} />
    );
  }, [onRemoveRank]);


  const targetColumns: ColumnMeta[] = [
    {
      bodyTemplate: channelNumberEditorBodyTemplate,
      field: 'user_Tvg_chno',
      filter: true,
      header: 'Ch.',
      isHidden: props.isAdditionalChannels === true,
      sortable: true,
      style: {
        maxWidth: '4rem',
        width: '4rem',
      } as CSSProperties,
    },
    {
      bodyTemplate: logoEditorBodyTemplate,
      field: 'user_Tvg_logo',
      fieldType: 'image',
      header: "Logo"
    },
    {
      bodyTemplate: channelNameEditorBodyTemplate,
      field: 'user_Tvg_name',
      header: 'Name',
    },
    epgColumnConfig,
    {
      bodyTemplate: sourceActionBodyTemplate,
      field: 'x',
      header: '',
      style: {
        maxWidth: '2rem',
        width: '2rem',
      } as CSSProperties,
    },
  ];


  return (
    <>
      <Toast position="bottom-right" ref={toast} />
      <DataSelectorPicker

        id={props.id + '-dataselectorpicker'}
        isLoading={videoStreamsQuery.isLoading}
        onSelectionChange={onChange}
        onTargetOnValueChanged={async (e) => {
          if (props.isAdditionalChannels === true) {
            const d = e as VideoStreamDto[];
            const newData = d.map((x: VideoStreamDto, index: number) => {
              return {
                ...x,
                rank: index,
              }
            }) as ChildVideoStreamDto[];
            props.onValueChanged?.(newData);
          } else {
            props.onValueChanged?.(e as ChildVideoStreamDto[]);
          }
        }}
        onTargetSelectionChange={async (e) => {
          if (props.isAdditionalChannels === true) {
            const d = e as VideoStreamDto[];
            const newData = d.map((x: VideoStreamDto, index: number) => {
              return {
                ...x,
                rank: index,
              }
            }) as ChildVideoStreamDto[];
            await onEdit(newData);
          }
        }
        }
        selection={targetVideoStreams}
        showUndo
        sourceColumns={sourceColumns}
        sourceDataSource={sourceVideoStreams}
        sourceEnableState={props.enableState}
        sourceHeaderTemplate={props.sourceHeaderTemplate}
        sourceName='Streams'
        sourceRightColSize={1}
        sourceSortField='user_Tvg_name'
        sourceStyle={{
          height: props.maxHeight !== null ? props.maxHeight : 'calc(100vh - 40px)',

        }}
        targetColumns={targetColumns}
        targetDataSource={targetVideoStreams}
        targetEnableState={props.enableState}
        targetName='Selected'
        targetReorderable={props.isAdditionalChannels}
        targetRightColSize={3}
        targetSortField={props.isAdditionalChannels === true ? 'rank' : 'user_Tvg_chno'}
      />
    </>
  );
};

PlayListDataSelectorPicker.displayName = 'PlayList Editor';
PlayListDataSelectorPicker.defaultProps = {
  enableState: true,
  isAdditionalChannels: false,
  maxHeight: null,
  showHidden: true,
  showTriState: true
};

export type PlayListDataSelectorPickerProps = {
  enableState?: boolean;
  id: string;
  isAdditionalChannels?: boolean;
  maxHeight?: number;
  onValueChanged?: (value: ChildVideoStreamDto[]) => void;
  showHidden?: boolean | undefined;
  showTriState?: boolean | null | undefined;
  sourceHeaderTemplate?: ReactNode | undefined;
  streamGroup?: StreamGroupDto | undefined;
  videoStream?: VideoStreamDto | undefined;
};

export default memo(PlayListDataSelectorPicker);
