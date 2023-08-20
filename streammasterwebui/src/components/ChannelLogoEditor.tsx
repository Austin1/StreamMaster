import React from "react";

import IconSelector from "./selectors/IconSelector";
import { type VideoStreamDto } from "../store/iptvApi";
import { type UpdateVideoStreamRequest } from "../store/iptvApi";
import { useVideoStreamsUpdateVideoStreamMutation } from "../store/iptvApi";


const ChannelLogoEditor = (props: StreamDataSelectorProps) => {
  const [videoStreamsUpdateVideoStreamMutation] = useVideoStreamsUpdateVideoStreamMutation();

  const onUpdateVideoStream = async (Logo: string) => {
    if (props.data.id === '') {
      return;
    }

    const data = {} as UpdateVideoStreamRequest;

    data.id = props.data.id;

    if (Logo && Logo !== '' && props.data.user_Tvg_logo !== Logo) {
      data.tvg_logo = Logo;
    }

    await videoStreamsUpdateVideoStreamMutation(data)
      .then(() => {

      }).catch((e) => {
        console.error(e);
      });

  };

  return (
    <IconSelector
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

ChannelLogoEditor.displayName = 'Logo Editor';
ChannelLogoEditor.defaultProps = {
  enableEditMode: true
};

export type StreamDataSelectorProps = {
  data: VideoStreamDto;
  enableEditMode?: boolean;
};

export default React.memo(ChannelLogoEditor);
