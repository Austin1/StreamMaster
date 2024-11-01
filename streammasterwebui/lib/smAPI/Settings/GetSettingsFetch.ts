import { GetSettings } from '@lib/smAPI/Settings/SettingsCommands';
import { Logger } from '@lib/common/logger';
import { createAsyncThunk } from '@reduxjs/toolkit';


export const fetchGetSettings = createAsyncThunk('cache/getGetSettings', async (_: void, thunkAPI) => {
  try {
    Logger.debug('Fetching GetSettings');
    const fetchDebug = localStorage.getItem('fetchDebug');
    const start = performance.now();
    const response = await GetSettings();
    if (fetchDebug) {
      const duration = performance.now() - start;
      Logger.debug(`Fetch GetSettings completed in ${duration.toFixed(2)}ms`);
    }

    return {param: _, value: response };
  } catch (error) {
    console.error('Failed to fetch', error);
    return thunkAPI.rejectWithValue({ error: error || 'Unknown error', value: undefined });
  }
});


