import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import * as SecureStore from 'expo-secure-store';
import type { AuthResponse, UserDto } from './api/authApi';

const REFRESH_TOKEN_KEY = 'stoq_refresh_token';

export interface AuthState {
  accessToken: string | null;
  user: UserDto | null;
  householdId: string | null;
  isAuthenticated: boolean;
}

const initialState: AuthState = {
  accessToken: null,
  user: null,
  householdId: null,
  isAuthenticated: false,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setCredentials(state, action: PayloadAction<AuthResponse>) {
      state.accessToken = action.payload.accessToken;
      state.user = action.payload.user;
      state.householdId = action.payload.user.householdId;
      state.isAuthenticated = true;
      SecureStore.setItemAsync(REFRESH_TOKEN_KEY, action.payload.refreshToken).catch(() => null);
    },
    setHousehold(state, action: PayloadAction<string>) {
      state.householdId = action.payload;
      if (state.user) {
        state.user = { ...state.user, householdId: action.payload };
      }
    },
    clearCredentials(state) {
      state.accessToken = null;
      state.user = null;
      state.householdId = null;
      state.isAuthenticated = false;
      SecureStore.deleteItemAsync(REFRESH_TOKEN_KEY).catch(() => null);
    },
  },
});

export const { setCredentials, setHousehold, clearCredentials } = authSlice.actions;
export default authSlice.reducer;

export const getStoredRefreshToken = (): Promise<string | null> =>
  SecureStore.getItemAsync(REFRESH_TOKEN_KEY);
