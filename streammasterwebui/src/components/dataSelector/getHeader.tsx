import { type ReactNode } from "react";

import { camel2title } from "../../common/common";
import { type ColumnFieldType } from "./DataSelectorTypes";

function getHeader(field: string, header: string | undefined, fieldType: ColumnFieldType | undefined): ReactNode {

  if (!fieldType) {
    return header ? header : camel2title(field);
  }

  switch (fieldType) {
    case 'blank':
      return <div />;
    case 'epg':
      return 'EPG';
    case 'm3ulink':
      return 'M3U';
    case 'epglink':
      return 'XMLTV';
    case 'url':
      return 'HDHR URL';
    case 'streams':
      return (
        <div

        >
          Streams<br />(active/total)
        </div>
      );
    default:
      return header ? header : camel2title(field);
  }
}

export default getHeader;
