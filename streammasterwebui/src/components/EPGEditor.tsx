import React from "react";

import { type VideoStreamDto } from "../store/iptvApi";
import { type UpdateVideoStreamRequest } from "../store/iptvApi";
import { useVideoStreamsUpdateVideoStreamMutation } from "../store/iptvApi";
import EPGSelector from "./selectors/EPGSelector";


const EPGEditor = (props: StreamDataSelectorProps) => {
  const [videoStreamsUpdateVideoStreamMutation] = useVideoStreamsUpdateVideoStreamMutation();

  const onUpdateVideoStream = async (epg: string) => {
    if (props.data.id === '') {
      return;
    }

    const data = {} as UpdateVideoStreamRequest;

    data.id = props.data.id;

    if (epg && epg !== '' && props.data.user_Tvg_ID !== epg) {
      data.tvg_ID = epg;
    }

    await videoStreamsUpdateVideoStreamMutation(data)
      .then(() => {

      }).catch((e) => {
        console.error(e);
      });

  };

  return (
    <EPGSelector
      className="p-inputtext-sm"
      enableEditMode={props.enableEditMode}
      onChange={
        async (e: string) => {
          await onUpdateVideoStream(e);
        }
      }
      value={props.data.user_Tvg_logo}
    />
  );
};

EPGEditor.displayName = 'EPG Editor';
EPGEditor.defaultProps = {
  enableEditMode: true
};

export type StreamDataSelectorProps = {
  data: VideoStreamDto;
  enableEditMode?: boolean;
};

export default React.memo(EPGEditor);
