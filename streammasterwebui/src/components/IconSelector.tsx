import { Button } from 'primereact/button';
import { type DropdownChangeEvent } from 'primereact/dropdown';
import { Dropdown } from 'primereact/dropdown';
import { classNames } from 'primereact/utils';
import * as React from 'react';
import { type IconFileDto } from '../store/iptvApi';
import { useIconsGetIconsQuery } from '../store/iptvApi';
import StreamMasterSetting from '../store/signlar/StreamMasterSetting';
import FileDialog from './FileDialog';
import { getTopToolOptions } from '../common/common';

import { ResetLogoIcon } from '../common/icons';
import { LazyLoadImage } from 'react-lazy-load-image-component';

const IconSelector = (props: IconSelectorProps) => {

  const [displayAdd, setDisplayAdd] = React.useState(false);
  const [selectedIcon, setSelectedIcon] = React.useState<IconFileDto>();
  const [resetIcon, setResetIcon] = React.useState<IconFileDto>();
  const setting = StreamMasterSetting();
  const icons = useIconsGetIconsQuery();


  React.useMemo(() => {
    if (!icons.data) {
      return;
    }

    if (props.value) {
      const tests = icons.data.filter((a: IconFileDto) => a.url == props.value);

      if (tests && tests != undefined && tests.length > 0) {
        setSelectedIcon(tests[0]);

        return;
      }
    }

    if (props.resetValue) {
      const tests = icons.data.filter((a: IconFileDto) => a.url == props.resetValue);

      if (tests && tests != undefined && tests.length > 0) {
        setResetIcon(tests[0]);

        return;
      }
    }

  }, [icons.data, props.value, props.resetValue]);


  const onChange = React.useCallback((event: DropdownChangeEvent) => {
    setSelectedIcon(event.value);

    if (!event.value) return;

    const icon = event.value as IconFileDto;

    props.onChange?.(icon);

  }, [props]);

  const selectedTemplate = React.useCallback((option: IconFileDto) => {
    if (!option || option.url == undefined || option.url == null || option.url == '') {
      return (<div />);
      // <QuestionMarkIcon className='flex col-1 m-0 text-red-500' sx={{ fontSize: 32 }} />
      // );
    }

    return (
      <div className='flex h-full justify-content-start align-items-center p-0 m-0 pl-2'>
        <LazyLoadImage
          alt={option.name}
          loading="lazy"
          src={option.url}
          style={{
            maxWidth: '1.2rem',
            objectFit: 'contain',
          }}
        />
      </div>
    );
  }, []);

  const iconOptionTemplate = React.useCallback((option: IconFileDto) => {
    return (
      <>
        <LazyLoadImage
          alt={option.name ?? 'name'}
          className="flex align-items-center max-w-full h-2rem text-base text-color surface-overlay appearance-none outline-none focus:border-primary"
          loading="lazy"

          src={option.url ?? setting.defaultIcon}
        />
        <div className="white-space-normal">{option.name}</div>
      </>
    );
  }, [setting.defaultIcon]);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars, @typescript-eslint/no-explicit-any
  const filterTemplate = React.useCallback((options: any) => {
    return (
      <div className="flex gap-2 align-items-center">
        <Button
          icon="pi pi-plus"
          onClick={() => setDisplayAdd(true)}
          rounded
          severity="success"
          size="small"
          style={{
            ...{
              maxHeight: "2rem",
              maxWidth: "2rem"
            }
          }}
          tooltip="Add Logo"
          tooltipOptions={getTopToolOptions}
        />

        {options.element}
        {/* {JSON.stringify(props.resetValue === props.value)}
        {JSON.stringify(props.resetValue)}
        {JSON.stringify(props.value)} */}
        {props.resetValue !== props.value && <Button
          icon={<ResetLogoIcon sx={{ fontSize: 18 }} />}
          onClick={() => {
            if (resetIcon !== undefined) {
              props.onReset(resetIcon)
            } else if (setting.defaultIconDto !== undefined) {
              props.onReset(setting.defaultIconDto)
            }
          }
          }
          rounded
          severity="warning"
          size="small"
          style={{
            ...{
              maxHeight: "2rem",
              maxWidth: "2rem"
            }
          }}
          tooltip="Reset Logo"
          tooltipOptions={getTopToolOptions}
        />
        }
      </div>
    );
  }, [props, resetIcon, setting.defaultIconDto]);


  const className = classNames('iconSelector p-0 m-0 w-full z-5 ', props.className, {
    'p-button-loading': props.loading,
    'p-disabled': props.disabled,
  });

  if (props.enableEditMode !== true) {
    return (
      <img
        alt='logo'
        className="max-h-1rem"
        onError={(e: React.SyntheticEvent<HTMLImageElement, Event>) => (e.currentTarget.src = (e.currentTarget.src = setting.defaultIcon))}
        src={selectedIcon?.url ?? setting.defaultIcon}
        style={{
          maxWidth: '1.5rem',
          objectFit: 'contain',
        }}
      />
    )
  }


  return (
    <>
      <FileDialog
        fileType="icon"
        onClose={() => setDisplayAdd(false)}
        show={displayAdd}
      />
      <div className="iconSelector flex w-full">
        <Dropdown
          className={className}
          disabled={props.disabled}
          filter
          filterBy="name"
          // filterInputAutoFocus
          filterTemplate={filterTemplate}
          itemTemplate={iconOptionTemplate}
          onChange={onChange}
          optionLabel="name"
          options={icons.data}
          placeholder="Select an Icon"
          style={{
            ...{
              backgroundColor: 'var(--mask-bg)',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
            },
          }}
          value={selectedIcon}
          valueTemplate={selectedTemplate}
          virtualScrollerOptions={{
            itemSize: 64,
          }}

        />
      </div>
    </>
  );
};

IconSelector.displayName = 'IconSelector';
IconSelector.defaultProps = {
  className: null,
  disabled: false,
  enableEditMode: true,
  loading: false
};

type IconSelectorProps = {
  className?: string | null;
  disabled?: boolean;
  enableEditMode?: boolean;
  loading?: boolean;
  onChange: ((value: IconFileDto) => void);
  onReset: ((value: IconFileDto) => void);
  resetValue: string;
  value: string;
};

export default React.memo(IconSelector);