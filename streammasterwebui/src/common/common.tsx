/* eslint-disable @typescript-eslint/no-explicit-any */
import * as React from 'react';

import { type TooltipOptions } from 'primereact/tooltip/tooltipoptions';
import { FormattedMessage, useIntl } from 'react-intl';

import { type IconFileDto } from '../store/iptvApi';
import { type VideoStreamDto } from '../store/iptvApi';
import { type ChildVideoStreamDto } from '../store/iptvApi';
import { baseHostURL, isDebug } from '../settings';
import { SMFileTypes } from '../store/streammaster_enums';

import ExportButton from '../components/export/ExportButton';
import GlobalSearch from '../components/search/GlobalSearch';
import { Checkbox } from 'primereact/checkbox';
import { type DataSelectorProps } from '../components/dataSelector/DataSelector';
import { type DataTableFilterMeta } from 'primereact/datatable';
import { type DataTableFilterMetaData } from 'primereact/datatable';
import { type ColumnMeta } from '../components/dataSelector/DataSelectorTypes';

export const getTopToolOptions = { autoHide: true, hideDelay: 100, position: 'top', showDelay: 400 } as TooltipOptions;
export const getLeftToolOptions = { autoHide: true, hideDelay: 100, position: 'left', showDelay: 400 } as TooltipOptions;

<FormattedMessage defaultMessage="Stream Master" id="app.title" />;


// export type MatchMode = 'between' | 'channelGroupsMatch' | 'contains' | 'custom' | 'dateAfter' | 'dateBefore' | 'dateIs' | 'dateIsNot' | 'endsWith' | 'equals' | 'gt' | 'gte' | 'in' | 'lt' | 'lte' | 'notContains' | 'notEquals' | 'startsWith' | undefined;
export type MatchMode = 'between' | 'contains' | 'custom' | 'dateAfter' | 'dateBefore' | 'dateIs' | 'dateIsNot' | 'endsWith' | 'equals' | 'gt' | 'gte' | 'in' | 'lt' | 'lte' | 'notContains' | 'notEquals' | 'startsWith' | undefined;

export function areFilterMetaEqual(a: DataTableFilterMeta, b: DataTableFilterMeta): boolean {
  const aKeys = Object.keys(a);
  const bKeys = Object.keys(b);

  // Compare if both objects have the same keys
  if (aKeys.length !== bKeys.length) {
    return false;
  }

  for (const key of aKeys) {
    if (!b[key]) {
      return false; // Key doesn't exist in 'b'
    }

    const aData = a[key] as SMDataTableFilterMetaData;
    const bData = b[key] as SMDataTableFilterMetaData;

    // Compare 'matchMode'
    if (aData.matchMode !== bData.matchMode) {
      return false;
    }

    // Compare 'value' (this assumes a simple comparison; for deep object comparison, consider using lodash's isEqual or similar)
    if (aData.value !== bData.value) {
      return false;
    }
  }

  return true;
}

export function toCamelCase(str: string): string {
  return str
    .trim()
    .split(/[\s_-]+/)
    .map((word, index) =>
      index === 0
        ? word.toLowerCase()
        : word.charAt(0).toUpperCase() + word.slice(1).toLowerCase()
    )
    .join('');
}

// export function GetMessagea(id: string): string {
//   const intl = useIntl();
//   const message = intl.formatMessage({ id: id });

//   return message;
// }

export function GetMessage(...args: string[]): string {
  const intl = useIntl();
  if (args === undefined || args.length === 0 || args[0] === '') {
    return '';
  }

  // const messageTest = toCamelCase(args.join());

  // var test = intl.formatMessage({ id: messageTest });
  // if (test !== messageTest) {
  //   return test;
  // }

  const ids: string[] = args.flatMap(arg => arg.split(' '));

  const message = ids.map(x => intl.formatMessage({ id: x })).join(' ');
  if (message === toCamelCase(message)) {
    return args.join('');
  }

  return message;
}


// export function GetMessage(id: string): string {
//   const intl = useIntl();
//   const message = intl.formatMessage({ id: id });

//   return message;
// }


export function addOrUpdateValueForField(
  data: SMDataTableFilterMetaData[],
  targetFieldName: string,
  matchMode: MatchMode,
  newValue: string
): void {

  // let itemFound = false;

  // data.forEach(item => {
  //   if (item.fieldName === targetFieldName) {
  //     item.matchMode = matchMode;
  //     item.value = newValue;
  //     item.valueType = typeof newValue;
  //     itemFound = true;
  //   }
  // });

  // if (!itemFound) {
  data.push({
    fieldName: targetFieldName,
    matchMode: matchMode as MatchMode,
    value: newValue
  });
  // }
}

export function areDataTableFilterMetaDataEqual(a: SMDataTableFilterMetaData, b: SMDataTableFilterMetaData): boolean {
  // Compare simple string properties
  if (a.fieldName !== b.fieldName) return false;
  if (a.matchMode !== b.matchMode) return false;

  // Deep comparison of 'value'. This assumes simple equality check; for objects or arrays, you might need a deeper comparison.
  if (a.value !== b.value) return false;

  return true;
}

export function areDataTableFilterMetaDatasEqual(arr1: SMDataTableFilterMetaData[], arr2: SMDataTableFilterMetaData[]): boolean {
  if (arr1.length !== arr2.length) return false;

  for (let i = 0; i < arr1.length; i++) {
    if (!areDataTableFilterMetaDataEqual(arr1[i], arr2[i])) {
      return false;
    }
  }

  return true;
}

export type SimpleQueryApiArg = {
  count?: number;
  first?: number;
  jsonArgumentString?: string | null;
  jsonFiltersString?: string | null;
  last?: number;
  name?: string;
  orderBy?: string;
  pageNumber?: number;
  pageSize?: number;
};

export type GetApiArg = {
  count?: number;
  first?: number;
  jsonArgumentString?: string | null;
  jsonFiltersString?: string | null;
  last?: number;
  name?: string;
  orderBy?: string;
  pageNumber?: number;
  pageSize?: number;
};

type QueryHookResult = {
  data?: string[];
  isError: boolean;
  isFetching: boolean;
  isLoading: boolean;
};

export type QueryHook = () => QueryHookResult;


export type HasId = {
  [key: string]: any;
  id: number | string;
}

export function compareIconFileDto(a: IconFileDto, b: IconFileDto): number {
  // Compare by id
  if (a.id !== undefined && b.id !== undefined) {
    if (a.id < b.id) return -1;
    if (a.id > b.id) return 1;
  }

  // Compare by source
  if (a.source !== undefined && b.source !== undefined) {
    const sourceComparison = a.source.localeCompare(b.source);
    if (sourceComparison !== 0) return sourceComparison;
  }

  // Compare by name
  if (a.name !== undefined && b.name !== undefined) {
    return a.name.localeCompare(b.name);
  }

  return 0;
}

export function areIconFileDtosEqual(array1: IconFileDto[], array2: IconFileDto[]): boolean {
  if (array1.length !== array2.length) {
    return false;
  }

  for (let i = 0; i < array1.length; i++) {
    if (compareIconFileDto(array1[i], array2[i]) !== 0) {
      return false;
    }
  }

  return true;
}

export type SMDataTableFilterMetaData = DataTableFilterMetaData & {
  fieldName: string;
  matchMode: MatchMode;
}

export function isChildVideoStreamDto(value: unknown): value is ChildVideoStreamDto {
  // Perform the necessary type checks to determine if 'value' is of type 'ChildVideoStreamDto'
  if (typeof value === 'object' && value !== null) {
    const dto = value as ChildVideoStreamDto;
    return (
      typeof dto.rank !== undefined
    );
  }

  return false;
}

export const GetMessageDiv = (id: string, upperCase?: boolean | null): React.ReactNode => {
  const intl = useIntl();
  const message = intl.formatMessage({ id: id });
  if (upperCase) {
    return <div>{message.toUpperCase()}</div>;
  }

  return <div>{message}</div>;
}

export function areVideoStreamsEqual(
  streams1: ChildVideoStreamDto[] | VideoStreamDto[],
  streams2: ChildVideoStreamDto[] | VideoStreamDto[]
): boolean {
  if (streams1.length !== streams2.length) {
    return false;
  }

  for (let i = 0; i < streams1.length; i++) {
    if (streams1[i].id !== streams2[i].id) {
      return false;
    }

    if (isChildVideoStreamDto(streams1[i]) && isChildVideoStreamDto(streams2[i])) {
      if ((streams1[i] as ChildVideoStreamDto).rank !== (streams2[i] as ChildVideoStreamDto).rank) {
        return false;
      }
    }
  }

  return true;
}


export function isValidUrl(string: string): boolean {
  try {
    new URL(string);
    return true;
  } catch (err) {
    return false;
  }
}


export async function copyTextToClipboard(text: string) {
  if ('clipboard' in navigator) {
    await navigator.clipboard.writeText(text);
    return;
  } else {
    return document.execCommand('copy', true, text);
  }
}

export type PropsComparator<C extends React.ComponentType> = (
  prevProps: Readonly<React.ComponentProps<C>>,
  nextProps: Readonly<React.ComponentProps<C>>,
) => boolean;



export const camel2title = (camelCase: string): string => camelCase
  .replace(/([A-Z])/g, (match) => ` ${match}`)
  .replace(/^./, (match) => match.toUpperCase())
  .trim();

export function formatJSONDateString(jsonDate: string | undefined): string {
  if (!jsonDate) return '';
  const date = new Date(jsonDate);
  const ret = date.toLocaleDateString('en-US', {
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    month: '2-digit',
    second: '2-digit',
    year: 'numeric',
  });

  return ret;
}

function getApiUrl(path: SMFileTypes, originalUrl: string): string {
  return `${isDebug ? baseHostURL : ''}/api/files/${path}/${encodeURIComponent(originalUrl)}`;
}

export const arraysMatch = (arr1: string[], arr2: string[]): boolean => {
  if (arr1.length !== arr2.length) {
    return false;
  }

  // Sort both arrays using localeCompare for proper string comparison
  const sortedArr1 = arr1.slice().sort((a, b) => a.localeCompare(b));
  const sortedArr2 = arr2.slice().sort((a, b) => a.localeCompare(b));

  // Compare the sorted arrays element by element
  for (let i = 0; i < sortedArr1.length; i++) {
    if (sortedArr1[i] !== sortedArr2[i]) {
      return false;
    }
  }

  return true;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function checkData(data: any): boolean {
  if (data === null || data === undefined || data.data === null || data.data === undefined) {
    return false;
  }

  return true;

}


export function getIconUrl(iconOriginalSource: string | null | undefined, defaultIcon: string, cacheIcon: boolean): string {
  if (!iconOriginalSource || iconOriginalSource === '') {
    iconOriginalSource = `${isDebug ? baseHostURL + '/' : '/'}${defaultIcon}`;
  }

  let originalUrl = iconOriginalSource;

  if (iconOriginalSource.startsWith('/')) {
    iconOriginalSource = iconOriginalSource.substring(1);
  }

  if (iconOriginalSource.startsWith('images/')) {
    iconOriginalSource = `${isDebug ? baseHostURL + '/' : ''}${iconOriginalSource}`;
  } else if (!iconOriginalSource.startsWith('http')) {
    iconOriginalSource = getApiUrl(SMFileTypes.TvLogo, originalUrl);
  } else if (cacheIcon) {
    iconOriginalSource = getApiUrl(SMFileTypes.Icon, originalUrl);
  }

  return iconOriginalSource;
}

export const removeQuotes = (str: string) => str.startsWith('"') && str.endsWith('"') ? str.slice(1, -1) : str;
export const hasColumns = (columns?: ColumnMeta[]) => columns && columns.length > 0;

export function isEmptyObject(value: any): boolean {
  // Check if value is an empty object
  if (value && typeof value === "object" && Object.keys(value).length === 0 && value.constructor === Object) {
    return true;
  }

  // Check if value is an empty array
  if (Array.isArray(value) && value.length === 0) {
    return true;
  }

  // Check if the first item of the array is empty (including empty object)
  if (Array.isArray(value) && value.length > 0 && (value[0] === undefined || value[0] === null || value[0] === "" || (typeof value[0] === "object" && Object.keys(value[0]).length === 0))) {
    return true;
  }

  return false;
}


type MultiSelectCheckboxProps = {
  onMultiSelectClick?: (value: boolean) => void,
  props: DataSelectorProps,
  rowClick: boolean,
  setRowClick: (val: boolean) => void
};

/**
 * MultiSelectCheckbox component is responsible for rendering and managing
 * the multi-select checkbox based on the provided selection mode.
 *
 * @param props The properties for the MultiSelectCheckbox component.
 */
export const MultiSelectCheckbox: React.FC<MultiSelectCheckboxProps> = (props) => {
  const { onMultiSelectClick, rowClick, setRowClick, props: dataSelectorProps } = props;

  return (
    <div hidden={dataSelectorProps.selectionMode !== 'selectable'}>
      <Checkbox
        checked={rowClick}
        onChange={(e) => {
          onMultiSelectClick?.(e.checked ?? false);
          setRowClick(e.checked ?? false);
        }}
        tooltip="Multi Select"
        tooltipOptions={getTopToolOptions}
      />
    </div>
  );
};


export const getColumnClass = (size?: number, secondSize?: number) => {
  if (size !== undefined) {
    return `col-${12 - size}`;
  }

  if (secondSize !== undefined) {
    return `col-${secondSize}`;
  }

  return 'col-6';
}

export const HeaderLeft: React.FC<{ props: any }> = ({ props }) => (
  <div className={`flex debug flex-nowrap justify-content-start header p-0 m-0 align-items-center ${props?.headerLeftTemplate ? getColumnClass(props.leftColSize, 4) : 'col-1'}`}>
    {props.headerLeftTemplate}
  </div>
);

export const GlobalSearchComponent: React.FC<{ clearSourceFilter: any, globalSearchName: string, globalSourceFilterValue: string, onGlobalSourceFilterChange: any, props: any }> = ({ clearSourceFilter, props, globalSearchName, globalSourceFilterValue, onGlobalSourceFilterChange }) => (
  props.globalSearchEnabled &&
  <GlobalSearch
    clearSourceFilter={clearSourceFilter}
    columns={props.columns}
    globalSearchName={globalSearchName}
    globalSourceFilterValue={globalSourceFilterValue}
    onGlobalSourceFilterChange={onGlobalSourceFilterChange}
  />
);

export const ExportComponent: React.FC<{ exportCSV: any }> = ({ exportCSV }) => (
  <ExportButton exportCSV={exportCSV} />
);

export type UserInformation = {
  IsAuthenticated: boolean;
  TokenAge: Date
}
