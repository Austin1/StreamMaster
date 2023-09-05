import { type PayloadAction } from '@reduxjs/toolkit';
import { createSlice } from '@reduxjs/toolkit';

import { type RootState } from '../store';
import { type StreamGroupDto } from '../../store/iptvApi';

type SetSelectedStreamGroupSlicePayload = {
  streamGroup: StreamGroupDto;
  typename: string;
}

type QueryFilterState = Record<string, StreamGroupDto>;

const initialState: QueryFilterState = {};

const selectedStreamGroupSlice = createSlice({
  initialState,
  name: 'selectedStreamGroup',
  reducers: {
    setSelectedStreamGroupInternal: (state, action: PayloadAction<SetSelectedStreamGroupSlicePayload>) => {
      const { typename, streamGroup } = action.payload;

      if (streamGroup !== null && streamGroup !== undefined) {
        state[typename] = streamGroup;
      } else {
        // eslint-disable-next-line @typescript-eslint/no-dynamic-delete
        delete state[typename]; // Remove the key if the filter is null or undefined
      }
    }
  }
});

export const selectedStreamGroup = (state: RootState, typename: number) => state.selectedStreamGroup[typename];
export const { setSelectedStreamGroupInternal } = selectedStreamGroupSlice.actions;
export default selectedStreamGroupSlice.reducer;
