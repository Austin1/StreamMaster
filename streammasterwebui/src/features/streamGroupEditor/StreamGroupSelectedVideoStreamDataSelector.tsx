import { Tooltip } from "primereact/tooltip";
import { memo, useCallback, useEffect, useMemo, type CSSProperties } from "react";
import { v4 as uuidv4 } from 'uuid';
import { useQueryAdditionalFilters } from "../../app/slices/useQueryAdditionalFilters";
import { useSelectedStreamGroup } from "../../app/slices/useSelectedStreamGroup";
import { GetMessage, getChannelGroupMenuItem, getColor } from "../../common/common";
import { GroupIcon } from "../../common/icons";
import { useChannelNameColumnConfig, useChannelNumberColumnConfig, useEPGColumnConfig } from "../../components/columns/columnConfigHooks";
import DataSelector from "../../components/dataSelector/DataSelector";
import { type ColumnMeta } from "../../components/dataSelector/DataSelectorTypes";
import { useStreamGroupVideoStreamsGetStreamGroupVideoStreamsQuery, type VideoStreamDto } from "../../store/iptvApi";
import StreamGroupChannelGroupsSelector from "./StreamGroupChannelGroupsSelector";
import VideoStreamRemoveFromStreamGroupDialog from "./VideoStreamRemoveFromStreamGroupDialog";

type StreamGroupSelectedVideoStreamDataSelectorProps = {
  readonly id: string;
};

const StreamGroupSelectedVideoStreamDataSelector = ({ id }: StreamGroupSelectedVideoStreamDataSelectorProps) => {
  const dataKey = id + '-StreamGroupSelectedVideoStreamDataSelector';
  const { selectedStreamGroup } = useSelectedStreamGroup(id);
  const enableEdit = true;

  const { columnConfig: channelNumberColumnConfig } = useChannelNumberColumnConfig({ useFilter: false });
  const { columnConfig: channelNameColumnConfig } = useChannelNameColumnConfig({ enableEdit: enableEdit, useFilter: false });
  const { columnConfig: epgColumnConfig } = useEPGColumnConfig({ enableEdit: enableEdit, useFilter: false });
  const { setQueryAdditionalFilter } = useQueryAdditionalFilters(dataKey);

  useEffect(() => {

    if (selectedStreamGroup !== undefined && selectedStreamGroup !== undefined && selectedStreamGroup.id > 0) {
      setQueryAdditionalFilter({ field: 'streamGroupId', matchMode: 'equals', values: [selectedStreamGroup.id.toString()] });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedStreamGroup]);

  const targetActionBodyTemplate = useCallback((data: VideoStreamDto) => {
    if (data.isReadOnly === true) {
      const tooltipClassName = "grouptooltip-" + uuidv4();
      // console.log(tooltipClassName)
      return (
        <div className='flex min-w-full min-h-full justify-content-end align-items-center'>
          <Tooltip position="left" target={"." + tooltipClassName} >
            {getChannelGroupMenuItem(data.channelGroupId, data.user_Tvg_group)}
          </Tooltip>
          <GroupIcon className={tooltipClassName} style={{ color: getColor(data.channelGroupId ?? 1) }} />
        </div >
      );
    }

    return (
      <div className='flex p-0 justify-content-end align-items-center'>
        <VideoStreamRemoveFromStreamGroupDialog id={id} value={data} />
      </div>
    );
  }, [id]);

  const targetColumns = useMemo((): ColumnMeta[] => {

    return [
      channelNumberColumnConfig,
      channelNameColumnConfig,
      epgColumnConfig,
      {
        bodyTemplate: targetActionBodyTemplate, field: 'Remove', header: '', resizeable: false, sortable: false,
        style: {
          maxWidth: '2rem',
        } as CSSProperties,
      }
    ]
  }, [channelNumberColumnConfig, channelNameColumnConfig, epgColumnConfig, targetActionBodyTemplate]);

  const rightHeaderTemplate = () => {
    return (
      <div className="flex justify-content-end align-items-center w-full gap-1" >
        <StreamGroupChannelGroupsSelector streamGroupId={selectedStreamGroup?.id} />
      </div>
    );
  }

  // const onRowReorder = async (changed: VideoStreamDto[]) => {

  //   const newData = changed.map((x: VideoStreamDto, index: number) => {
  //     return {
  //       rank: index,
  //       videoStreamId: x.id,
  //     }
  //   }) as VideoStreamIsReadOnly[];


  //   var toSend = {} as SetVideoStreamRanksRequest;

  //   toSend.streamGroupId = selectedStreamGroup.id;
  //   toSend.videoStreamIDRanks = newData;

  //   await streamGroupVideoStreamsSetVideoStreamRanksMutation(toSend)
  //     .then(() => {

  //     }).catch(() => {
  //       console.log('error');
  //     });

  // }

  return (
    <DataSelector
      columns={targetColumns}
      defaultSortField="user_tvg_name"
      emptyMessage="No Streams"
      headerName={GetMessage('streams')}
      headerRightTemplate={rightHeaderTemplate()}
      id={dataKey}
      key='rank'
      queryFilter={useStreamGroupVideoStreamsGetStreamGroupVideoStreamsQuery}
      selectionMode='single'
      style={{ height: 'calc(100vh - 40px)' }
      }
    />
  );
}

StreamGroupSelectedVideoStreamDataSelector.displayName = 'Stream Editor';

export default memo(StreamGroupSelectedVideoStreamDataSelector);


