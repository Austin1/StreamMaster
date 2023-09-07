import React from "react";
import InfoMessageOverLayDialog from "../InfoMessageOverLayDialog";
import { InputText } from "primereact/inputtext";
import { Accordion, AccordionTab } from 'primereact/accordion';
import PlayListDataSelector from "../../features/playListEditor/PlayListDataSelector";
import AddButton from "../buttons/AddButton";
import { type ChannelGroupDto, type StreamGroupsGetStreamGroupsApiArg, type AddStreamGroupRequest, useStreamGroupsAddStreamGroupMutation } from "../../store/iptvApi";
import { useStreamGroupsGetStreamGroupsQuery } from "../../store/iptvApi";

const StreamGroupAddDialog = (props: StreamGroupAddDialogProps) => {
  const [showOverlay, setShowOverlay] = React.useState<boolean>(false);
  const [block, setBlock] = React.useState<boolean>(false);
  const [infoMessage, setInfoMessage] = React.useState('');
  const [name, setName] = React.useState<string>('');
  const [streamGroupNumber, setStreamGroupNumber] = React.useState<number>();
  const [selectedChannelGroups, setSelectedChannelGroups] = React.useState<ChannelGroupDto[]>([] as ChannelGroupDto[]);

  const streamGroupsQuery = useStreamGroupsGetStreamGroupsQuery({} as StreamGroupsGetStreamGroupsApiArg);
  const [streamGroupsAddStreamGroupMutation] = useStreamGroupsAddStreamGroupMutation();

  const getNextStreamGroupNumber = React.useCallback((): number => {
    if (!streamGroupsQuery?.data?.data) {
      return 0;
    }

    if (streamGroupsQuery.data.data.length === 0) {
      return 1;
    }

    const numbers = streamGroupsQuery.data.data.map((x) => x.streamGroupNumber);

    const [min, max] = [1, Math.max(...numbers)];

    if (max < min) {
      return min;
    }

    const out = Array.from(Array(max - min), (v, i) => i + min).filter(
      (i) => !numbers.includes(i),
    );

    if (out.length > 0) {
      return out[0];
    }

    return max + 1;
  }, [streamGroupsQuery.data]);

  React.useEffect(() => {
    if (streamGroupNumber === undefined || streamGroupNumber === 0) {
      setStreamGroupNumber(getNextStreamGroupNumber());
    }
  }, [getNextStreamGroupNumber, streamGroupNumber]);

  const ReturnToParent = React.useCallback(() => {
    setSelectedChannelGroups([] as ChannelGroupDto[]);
    setShowOverlay(false);
    setInfoMessage('');
    setName('');
    setStreamGroupNumber(0);
    setBlock(false);
    props.onHide?.();
  }, [props]);


  const isSaveEnabled = React.useMemo((): boolean => {

    if (name && name !== '') {
      return true;
    }

    if (streamGroupNumber !== undefined && streamGroupNumber !== 0) {
      return true;
    }

    return false;

  }, [name, streamGroupNumber]);


  const onAdd = React.useCallback(() => {

    setBlock(true);

    if (!isSaveEnabled || !name || streamGroupNumber === 0 || name === '') {
      ReturnToParent();

      return;
    }


    if (!isSaveEnabled || !streamGroupNumber || streamGroupNumber === 0) {
      return;
    }

    const data = {} as AddStreamGroupRequest;

    data.name = name;
    data.streamGroupNumber = streamGroupNumber;

    if (selectedChannelGroups.length > 0) {
      data.channelGroupIds = selectedChannelGroups.map((x) => x.id??0);
    }

    streamGroupsAddStreamGroupMutation(data)
      .then(() => {
        setInfoMessage('Stream Group Added Successfully');
      }).catch((e) => {
        setInfoMessage('Stream Group Add Error: ' + e.message);
      });
  }, [ReturnToParent, isSaveEnabled, name, selectedChannelGroups, streamGroupNumber, streamGroupsAddStreamGroupMutation]);

  React.useEffect(() => {
    const callback = (event: KeyboardEvent) => {
      if (event.code === 'Enter' || event.code === 'NumpadEnter') {
        event.preventDefault();

        if (name !== "") {
          onAdd();
        }
      }

    };

    document.addEventListener('keydown', callback);

    return () => {
      document.removeEventListener('keydown', callback);
    };
  }, [onAdd, name]);

  const onsetSelectedChannelGroups = React.useCallback((selectedData: ChannelGroupDto | ChannelGroupDto[]) => {
    if (Array.isArray(selectedData)) {
      setSelectedChannelGroups(selectedData);
    } else {
      setSelectedChannelGroups([selectedData]);
    }

  }, []);

  return (
    <>

      <InfoMessageOverLayDialog
        blocked={block}
        closable
        header='Add Stream Group'
        infoMessage={infoMessage}
        onClose={() => {
          ReturnToParent();
        }}
        show={showOverlay}
      >

        <div className="justify-content-between align-items-center ">
          <div className="flex">
            <span className="p-float-label col-6">
              <InputText
                autoFocus
                className={name === '' ? 'withpadding p-invalid' : 'withpadding'}
                id="Name"
                onChange={(e) => setName(e.target.value)}
                type="text"
                value={name}
              />
              <label
                className="text-500"
                htmlFor="Name"
              >Name</label>
            </span>


          </div>

          <Accordion className='mt-2'>
            <AccordionTab header="Groups">
              <div className='col-12 m-0 p-0 pr-1' >
                <PlayListDataSelector
                  hideAddRemoveControls
                  id='streamggroupadddialog'
                  maxHeight={400}
                  onSelectionChange={(e) => onsetSelectedChannelGroups(e as ChannelGroupDto[])}
                />
              </div>
            </AccordionTab>

          </Accordion>

          <div className="flex col-12 mt-3 gap-2 justify-content-end">
            <AddButton label='Add Stream Group' onClick={() => onAdd()} tooltip='Add Stream Group' />
          </div>

        </div>
      </InfoMessageOverLayDialog>

      <AddButton onClick={() => setShowOverlay(true)} tooltip='Add Stream Group' />

    </>
  );
}

StreamGroupAddDialog.displayName = 'StreamGroupAddDialog';
StreamGroupAddDialog.defaultProps = {
}

type StreamGroupAddDialogProps = {
  readonly onHide?: () => void;
};

export default React.memo(StreamGroupAddDialog);
