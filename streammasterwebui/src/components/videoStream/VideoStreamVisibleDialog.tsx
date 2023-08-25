
import { useState, useCallback, useMemo, memo } from "react";
import { getTopToolOptions } from "../../common/common";
import { type VideoStreamsUpdateAllVideoStreamsFromParametersApiArg } from "../../store/iptvApi";
import { type VideoStreamDto, type UpdateVideoStreamsRequest, type UpdateVideoStreamRequest, useVideoStreamsUpdateAllVideoStreamsFromParametersMutation } from "../../store/iptvApi";
import { useVideoStreamsUpdateVideoStreamsMutation } from "../../store/iptvApi";
import InfoMessageOverLayDialog from "../InfoMessageOverLayDialog";
import { Button } from "primereact/button";
import { useQueryFilter } from "../../app/slices/useQueryFilter";

const VideoStreamVisibleDialog = (props: VideoStreamVisibleDialogProps) => {
  const [showOverlay, setShowOverlay] = useState<boolean>(false);
  const [block, setBlock] = useState<boolean>(false);
  const [infoMessage, setInfoMessage] = useState('');
  const [selectedVideoStreams, setSelectedVideoStreams] = useState<VideoStreamDto[]>([] as VideoStreamDto[]);
  const { queryFilter } = useQueryFilter(props.id);

  const [videoStreamsUpdateVideoStreams] = useVideoStreamsUpdateVideoStreamsMutation();
  const [videoStreamsUpdateAllVideoStreamsFromParametersMutation] = useVideoStreamsUpdateAllVideoStreamsFromParametersMutation();

  const ReturnToParent = useCallback(() => {
    setShowOverlay(false);
    setInfoMessage('');
    setBlock(false);
    props.onClose?.();
  }, [props]);

  useMemo(() => {

    if (props.values !== null && props.values !== undefined) {
      setSelectedVideoStreams(props.values);
    }

  }, [props.values]);


  const getTotalCount = useMemo(() => {
    if (props.overrideTotalRecords !== undefined) {
      return props.overrideTotalRecords;
    }

    return selectedVideoStreams.length;

  }, [props.overrideTotalRecords, selectedVideoStreams.length]);


  const onVisiblesClick = useCallback(async () => {
    setBlock(true);
    if (setSelectedVideoStreams.length === 0 || !queryFilter) {
      ReturnToParent();
      return;
    }

    if (props.selectAll === true) {
      const toSendAll = {} as VideoStreamsUpdateAllVideoStreamsFromParametersApiArg;

      toSendAll.parameters = queryFilter;
      toSendAll.parameters.pageSize = getTotalCount;

      toSendAll.request = {
        isHidden: true
      } as UpdateVideoStreamRequest;


      videoStreamsUpdateAllVideoStreamsFromParametersMutation(toSendAll)
        .then(() => {
          setInfoMessage('Set Stream Visibility Successfully');
        }
        ).catch((error) => {
          setInfoMessage('Set Stream Visibility Error: ' + error.message);
        });
      return;
    }

    const toSend = {} as UpdateVideoStreamsRequest;

    toSend.videoStreamUpdates = selectedVideoStreams.map((a) => {
      return {
        id: a.id,
        isHidden: !a.isHidden
      } as UpdateVideoStreamRequest;
    });


    videoStreamsUpdateVideoStreams(toSend)
      .then(() => {
        setInfoMessage('Set Stream Visibility Successfully');
      }
      ).catch((error) => {
        setInfoMessage('Set Stream Visibility Error: ' + error.message);
      });

  }, [ReturnToParent, getTotalCount, props.selectAll, queryFilter, selectedVideoStreams, videoStreamsUpdateAllVideoStreamsFromParametersMutation, videoStreamsUpdateVideoStreams]);


  if (props.skipOverLayer === true) {
    return (
      <Button
        disabled={selectedVideoStreams.length === 0}
        icon="pi pi-power-off"
        onClick={async () => await onVisiblesClick()}
        rounded
        severity="info"
        size="small"
        text={props.iconFilled !== true}
        tooltip="Set Visibilty"
        tooltipOptions={getTopToolOptions}
      />
    );
  }



  return (
    <>

      <InfoMessageOverLayDialog
        blocked={block}
        header={`Toggle visibility for ${getTotalCount < 2 ? getTotalCount + ' Stream ?' : getTotalCount + ' Streams ?'}`}
        infoMessage={infoMessage}
        onClose={() => { ReturnToParent(); }}
        show={showOverlay}
      >

        <div className='m-0 p-0 border-1 border-round surface-border'>
          <div className='m-3'>
            <h3 />

            <div className="card flex mt-3 flex-wrap gap-2 justify-content-center">
              <Button
                icon="pi pi-times "
                label="Cancel"
                onClick={(() => ReturnToParent())}
                rounded
                severity="warning"
              />
              <Button
                icon="pi pi-check"
                label="Change"
                onClick={async () => await onVisiblesClick()}
                rounded
                severity="success"
              />
            </div>
          </div>
        </div>

      </InfoMessageOverLayDialog >

      <Button
        disabled={selectedVideoStreams.length === 0 && props.selectAll !== true}
        icon="pi pi-power-off"
        onClick={() => setShowOverlay(true)}
        rounded
        severity="info"
        size="small"
        text={props.iconFilled !== true}
        tooltip="Set Visibilty"
        tooltipOptions={getTopToolOptions}
      />

    </>
  );
}

VideoStreamVisibleDialog.displayName = 'VideoStreamVisibleDialog';
VideoStreamVisibleDialog.defaultProps = {
  iconFilled: true,
  values: null,
}

type VideoStreamVisibleDialogProps = {
  iconFilled?: boolean;
  id: string;
  onClose?: (() => void);
  overrideTotalRecords?: number | undefined;
  selectAll?: boolean;
  skipOverLayer?: boolean;
  values?: VideoStreamDto[] | undefined;
};

export default memo(VideoStreamVisibleDialog);
