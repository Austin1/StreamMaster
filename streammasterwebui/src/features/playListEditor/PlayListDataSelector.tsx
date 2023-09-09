import { memo, useCallback, useEffect, useMemo, useState, type CSSProperties } from "react";
import { useShowHidden } from "../../app/slices/useShowHidden";
import { isEmptyObject } from "../../common/common";
import ChannelGroupAddDialog from "../../components/channelGroups/ChannelGroupAddDialog";
import ChannelGroupDeleteDialog from "../../components/channelGroups/ChannelGroupDeleteDialog";
import ChannelGroupEditDialog from "../../components/channelGroups/ChannelGroupEditDialog";
import ChannelGroupVisibleDialog from "../../components/channelGroups/ChannelGroupVisibleDialog";
import DataSelector from "../../components/dataSelector/DataSelector";
import { type ColumnMeta } from "../../components/dataSelector/DataSelectorTypes";
import { TriSelect } from "../../components/selectors/TriSelect";
import { useChannelGroupsGetChannelGroupsQuery, type ChannelGroupDto } from "../../store/iptvApi";


const PlayListDataSelector = (props: PlayListDataSelectorProps) => {
  const dataKey = props.id + '-PlayListDataSelector';
  const { showHidden, setShowHidden } = useShowHidden(dataKey);
  const [selectedChannelGroups, setSelectedChannelGroups] = useState<ChannelGroupDto[]>([] as ChannelGroupDto[]);

  useEffect(() => {
    if (showHidden === undefined) {
      setShowHidden(null);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [showHidden]);

  const onSelectionChange = (data: ChannelGroupDto[]) => {
    setSelectedChannelGroups(data);
    props.onSelectionChange?.(data);
  };

  const sourceActionBodyTemplate = useCallback((data: ChannelGroupDto) => (

    <div className='flex p-0 justify-content-end align-items-center'>

      <div hidden={data.isReadOnly === true && props.useReadOnly}>
        <ChannelGroupDeleteDialog iconFilled={false} id={dataKey} values={[data]} />
      </div>

      <ChannelGroupEditDialog value={data} />
      <ChannelGroupVisibleDialog id={dataKey} skipOverLayer value={[data]} />

    </div>

  ), [dataKey, props.useReadOnly]);

  const sourceColumns = useMemo((): ColumnMeta[] => {
    return [
      { field: 'name', filter: true, sortable: true },
      {
        field: 'streams', fieldType: 'streams', header: "Streams (active/total)",
        style: {
          maxWidth: '6rem',
          width: '6rem',
        } as CSSProperties,
      },
      {
        align: 'right',
        bodyTemplate: sourceActionBodyTemplate, field: 'isHidden', fieldType: 'isHidden', header: 'Actions',
        style: {
          maxWidth: '8rem',
          width: '8rem',
        } as CSSProperties,
      },
    ]
  }, [sourceActionBodyTemplate]);

  const sourceRightHeaderTemplate = useCallback(() => {

    return (
      <div className="flex justify-content-end align-items-center w-full gap-1">

        {props.hideControls !== true &&
          <>
            <TriSelect dataKey={dataKey} />
            <ChannelGroupVisibleDialog id={dataKey} value={selectedChannelGroups} />
            <ChannelGroupDeleteDialog iconFilled id={dataKey} values={selectedChannelGroups} />
          </>
        }

        <ChannelGroupAddDialog />
      </div>
    );
  }, [props.hideControls, selectedChannelGroups, dataKey]);


  return (

    <DataSelector
      columns={sourceColumns}
      emptyMessage="No Groups"
      headerName={props.name === undefined ? 'Playlist' : props.name}
      headerRightTemplate={props.hideAddRemoveControls === true ? null : sourceRightHeaderTemplate()}
      hideControls={props.hideControls}
      id={dataKey}
      onSelectionChange={(e) => {
        if (!isEmptyObject(e)) {
          onSelectionChange(e as ChannelGroupDto[]);
        } else {
          onSelectionChange([] as ChannelGroupDto[]);
        }
      }}
      queryFilter={useChannelGroupsGetChannelGroupsQuery}
      selectionMode='multiple'
      style={{
        height: props.maxHeight !== null ? props.maxHeight : 'calc(100vh - 40px)',
      }}
    />

  );
};

PlayListDataSelector.displayName = 'Play List Editor';
PlayListDataSelector.defaultProps = {
  hideAddRemoveControls: false,
  hideControls: false,
  maxHeight: null,
  name: 'Playlist',
  useReadOnly: true
};

export type PlayListDataSelectorProps = {
  readonly hideAddRemoveControls?: boolean;
  readonly hideControls?: boolean;
  readonly id: string;
  readonly maxHeight?: number;
  readonly name?: string;
  readonly onSelectionChange?: (value: ChannelGroupDto | ChannelGroupDto[]) => void;
  readonly useReadOnly?: boolean;
};

export default memo(PlayListDataSelector);
