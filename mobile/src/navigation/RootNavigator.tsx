import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { NavigationContainer } from '@react-navigation/native';
import { ActivityIndicator, View } from 'react-native';
import type { RootState, AppDispatch } from '../store';
import { setCredentials, getStoredRefreshToken } from '../store/authSlice';
import { authApi } from '../store/api/authApi';
import AuthNavigator from './AuthNavigator';
import HouseholdSetupNavigator from './HouseholdSetupNavigator';
import MainTabNavigator from './MainTabNavigator';

export default function RootNavigator() {
  const dispatch = useDispatch<AppDispatch>();
  const isAuthenticated = useSelector((state: RootState) => state.auth.isAuthenticated);
  const householdId = useSelector((state: RootState) => state.auth.householdId);
  const [isBootstrapping, setIsBootstrapping] = useState(true);

  // On cold start: attempt to restore session from the persisted refresh token.
  useEffect(() => {
    let cancelled = false;

    async function restoreSession() {
      try {
        const refreshToken = await getStoredRefreshToken();
        if (!refreshToken || cancelled) { return; }

        const result = await dispatch(
          authApi.endpoints.refresh.initiate({
            expiredAccessToken: '',
            refreshToken,
          }),
        ).unwrap();

        if (!cancelled) {
          dispatch(setCredentials(result));
        }
      } catch {
        // Refresh failed — user lands on auth screens.
      } finally {
        if (!cancelled) setIsBootstrapping(false);
      }
    }

    void restoreSession();
    return () => { cancelled = true; };
  }, [dispatch]);

  if (isBootstrapping) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator size="large" />
      </View>
    );
  }

  return (
    <NavigationContainer>
      {!isAuthenticated
        ? <AuthNavigator />
        : householdId
          ? <MainTabNavigator />
          : <HouseholdSetupNavigator />}
    </NavigationContainer>
  );
}
